using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StreetDiceGreyboxController
{
    [Serializable] private sealed class DiceSaleDto
    {
        public string sellerId = "", defaultBuyerId = "", winnerId = "";
        public bool isOpen, forcedDefaultWon, comeOutProtectionActive;
        public int winningAmount;
        public DiceSaleBidDto[] bids = Array.Empty<DiceSaleBidDto>();
    }

    [Serializable] private sealed class DiceSaleBidDto
    {
        public string playerId = "";
        public int amount;
        public bool isDefault;
    }

    [Serializable] private sealed class DiceSaleBidRequestDto
    {
        public string playerId = "", playerSessionToken = "";
        public int amount;
    }

    private readonly List<string> localTurnOrder = new();
    private DiceSaleDto activeSale;
    private float localSaleClosesAt, saleBotBidAt, saleAnnouncementUntil;
    private double onlineSaleRemainingMilliseconds;
    private string saleBidDigits = "2", saleAnnouncement = "";
    private string localSoldSellerId = "", localSoldBuyerId = "";
    private bool saleRequestInFlight;

    private bool SaleOpen => activeSale != null && activeSale.isOpen && phase == "SellingDice";
    private int HighestSaleBid
    {
        get
        {
            int highest = 0;
            if (activeSale?.bids != null)
                foreach (var bid in activeSale.bids) highest = Mathf.Max(highest, bid.amount);
            return highest;
        }
    }

    private void ResetDiceSale()
    {
        activeSale = null;
        saleRequestInFlight = false;
        localSoldSellerId = localSoldBuyerId = saleAnnouncement = "";
        saleAnnouncementUntil = 0f;
        localTurnOrder.Clear();
        localTurnOrder.AddRange(DemoShooterOrder);
    }

    private void SellCurrentDice()
    {
        if (rolling || shotCommitted || point != "-" || SaleOpen ||
            phase != "ComeOut" && phase != "ShooterDecision" && phase != "Lobby" && phase != "CeeLo") return;
        if (!localDemo)
        {
            if (shooterId != SelfId || !playerTokens.TryGetValue(SelfId, out var token)) return;
            StartCoroutine(Post("/api/street-dice/" + gameId + "/sell",
                JsonUtility.ToJson(new OnlineWalletRequest { playerId = SelfId, playerSessionToken = token })));
            return;
        }

        if (localTurnOrder.Count < 3) { result = "Three players are needed to sell the dice."; return; }
        string defaultBuyer = catcherId;
        if (defaultBuyer == shooterId || Balance(defaultBuyer) < 1)
        {
            foreach (var id in localTurnOrder)
                if (id != shooterId && Balance(id) >= 1) { defaultBuyer = id; break; }
        }
        if (defaultBuyer == shooterId || Balance(defaultBuyer) < 1)
        { result = "A catcher needs $1 for the default bid."; return; }
        activeSale = new DiceSaleDto
        {
            sellerId = shooterId, defaultBuyerId = defaultBuyer, isOpen = true,
            bids = new[] { new DiceSaleBidDto { playerId = defaultBuyer, amount = 1, isDefault = true } }
        };
        localSaleClosesAt = Time.unscaledTime + 5f;
        saleBotBidAt = Time.unscaledTime + 2f;
        saleBidDigits = "2";
        phase = "SellingDice";
        awaitingShootChoice = false;
        result = "Waiting for bidders. The catcher holds the $1 default bid.";
    }

    private void UpdateDiceSale()
    {
        if (!localDemo || !SaleOpen) return;
        if (Time.unscaledTime >= saleBotBidAt)
        {
            saleBotBidAt = float.PositiveInfinity;
            foreach (var id in localTurnOrder)
            {
                if (id == "p1" || id == activeSale.sellerId || Balance(id) <= HighestSaleBid) continue;
                if (random.NextDouble() > 0.38) continue;
                int bid = Mathf.Min(Balance(id), HighestSaleBid + random.Next(1, 11));
                AddLocalSaleBid(id, bid);
            }
        }
        if (Time.unscaledTime >= localSaleClosesAt) ResolveLocalDiceSale();
    }

    private void AddLocalSaleBid(string playerId, int amount)
    {
        if (!SaleOpen || playerId == activeSale.sellerId || amount <= HighestSaleBid || Balance(playerId) < amount) return;
        var bids = new List<DiceSaleBidDto>(activeSale.bids)
        { new DiceSaleBidDto { playerId = playerId, amount = amount } };
        activeSale.bids = bids.ToArray();
    }

    private void PlaceDiceSaleBid()
    {
        if (!SaleOpen || activeSale.sellerId == SelfId || saleRequestInFlight ||
            !int.TryParse(saleBidDigits, out int amount) || amount <= HighestSaleBid) return;
        int available = localDemo ? Balance(SelfId) : onlineAvailableBalance;
        if (amount > available) { result = "Not enough play money for that bid."; return; }
        if (localDemo) { AddLocalSaleBid(SelfId, amount); return; }
        if (!playerTokens.TryGetValue(SelfId, out var token)) return;
        StartCoroutine(SendDiceSaleBid(amount, token));
    }

    private IEnumerator SendDiceSaleBid(int amount, string token)
    {
        saleRequestInFlight = true;
        var request = new DiceSaleBidRequestDto { playerId = SelfId, playerSessionToken = token, amount = amount };
        yield return Post("/api/street-dice/" + gameId + "/sell/bid", JsonUtility.ToJson(request));
        saleRequestInFlight = false;
    }

    private void ResolveLocalDiceSale()
    {
        var sale = activeSale;
        if (sale == null || !sale.isOpen) return;
        DiceSaleBidDto winner = sale.bids[0];
        foreach (var bid in sale.bids) if (bid.amount > winner.amount) winner = bid;
        sale.isOpen = false;
        sale.winnerId = winner.playerId;
        sale.winningAmount = winner.amount;
        sale.forcedDefaultWon = winner.isDefault;
        sale.comeOutProtectionActive = winner.isDefault;
        TransferCash(winner.playerId, sale.sellerId, winner.amount);
        localSoldSellerId = sale.sellerId;
        localSoldBuyerId = winner.playerId;

        int sellerIndex = localTurnOrder.IndexOf(sale.sellerId);
        var successors = new List<string>();
        for (int offset = 1; offset < localTurnOrder.Count; offset++)
            successors.Add(localTurnOrder[(sellerIndex + offset) % localTurnOrder.Count]);
        localTurnOrder.Clear();
        localTurnOrder.Add(winner.playerId);
        foreach (var id in successors) if (id != winner.playerId) localTurnOrder.Add(id);
        localTurnOrder.Add(sale.sellerId);

        shooterId = winner.playerId;
        catcherId = "";
        foreach (var id in localTurnOrder)
            if (id != shooterId && id != sale.sellerId && Balance(id) >= 1) { catcherId = id; break; }
        shotAmount = 20;
        point = "-";
        activePointGroup = "-";
        streak = winner.isDefault ? 1.5f : 0f;
        shotCommitted = false;
        awaitingShootChoice = true;
        phase = gameMode == GameMode.CeeLo ? "CeeLo" : "ComeOut";
        rollState = RollState.WaitingForShot;
        ResetDiceToShooter();
        ApplyDiceColor();
        saleAnnouncement = SeatLabel(winner.playerId) + " bought the dice for $" + winner.amount;
        saleAnnouncementUntil = Time.unscaledTime + 4f;
        result = saleAnnouncement;
        nextBotAt = Time.time + 4f;
    }

    private void DrawDiceSale(float width)
    {
        // The sale headline and "bought the dice" announcement used to be a
        // plain cyan GUI.Label -- default skin font, small, no animation. "COME
        // OUT" paints onto the door itself. Both now use the same treatment.
        if (!SaleOpen)
        {
            if (Time.unscaledTime < saleAnnouncementUntil)
                DrawDoorGhostNumber(saleAnnouncement, 1f, new Color(0.92f, 0.95f, 0.92f));
            return;
        }
        float remaining = localDemo ? Mathf.Max(0, localSaleClosesAt - Time.unscaledTime) :
            Mathf.Max(0, (float)(onlineSaleRemainingMilliseconds / 1000d));
        DrawDoorGhostNumber(activeSale.sellerId == SelfId ? "WAITING FOR BIDDER" : "DICE FOR SALE",
            1f, new Color(0.92f, 0.95f, 0.92f));
        var headline = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 29 };
        headline.normal.textColor = new Color(0.7f, 0.96f, 1f);
        GUI.Label(new Rect(width / 2f - 170f, 148f, 340f, 32f),
            "$" + HighestSaleBid + "    " + Mathf.CeilToInt(remaining) + "s", headline);
        if (activeSale.sellerId == SelfId) return;

        float x = width / 2f - 108f, y = 190f;
        var numberStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 33 };
        numberStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y - 8f, 216f, 44f), "$" + saleBidDigits, numberStyle);
        string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "<" };
        var metal = Resources.Load<Texture2D>("UI/metal-plate-row-v1");
        for (int i = 0; i < keys.Length; i++)
        {
            var rect = new Rect(x + (i % 3) * 72f, y + 43f + (i / 3) * 57f, 64f, 52f);
            GUI.DrawTexture(rect, metal, ScaleMode.StretchToFill, true);
            if (!GUI.Button(rect, new GUIContent("", keys[i]), GUIStyle.none)) continue;
            if (keys[i] == "C") saleBidDigits = "";
            else if (keys[i] == "<") saleBidDigits = saleBidDigits.Length > 0 ? saleBidDigits.Substring(0, saleBidDigits.Length - 1) : "";
            else if (saleBidDigits.Length < 9) saleBidDigits = (saleBidDigits + keys[i]).TrimStart('0');
        }
        for (int i = 0; i < keys.Length; i++)
        {
            var rect = new Rect(x + (i % 3) * 72f, y + 43f + (i / 3) * 57f, 64f, 52f);
            GUI.Label(rect, keys[i], numberStyle);
        }
        var bidButton = new Rect(x, y + 282f, 208f, 50f);
        GUI.DrawTexture(bidButton, metal, ScaleMode.StretchToFill, true);
        GUI.enabled = !saleRequestInFlight && int.TryParse(saleBidDigits, out int amount) &&
            amount > HighestSaleBid && amount >= 1 && amount <= (localDemo ? Balance(SelfId) : onlineAvailableBalance);
        if (GUI.Button(bidButton, new GUIContent("", "Place bid"), GUIStyle.none)) PlaceDiceSaleBid();
        GUI.Label(bidButton, "BID $" + saleBidDigits, numberStyle);
        GUI.enabled = true;
    }
}
