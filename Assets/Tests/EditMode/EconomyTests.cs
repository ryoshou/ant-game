using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class EconomyTests
    {
        private static UpgradeBook NewBook() => new UpgradeBook(TuningConfig.CreateDefault());

        private static Wallet WalletWith(int dirt)
        {
            var wallet = new Wallet();
            wallet.Add(dirt);
            return wallet;
        }

        [Test]
        public void NextCost_FollowsSpecFormula()
        {
            UpgradeBook book = NewBook();

            Assert.AreEqual(20, book.NextCost(UpgradeId.WalkSpeed));
            Assert.AreEqual(20, book.NextCost(UpgradeId.DigSpeed));
            Assert.AreEqual(40, book.NextCost(UpgradeId.CarryAmount));
            Assert.AreEqual(50, book.NextCost(UpgradeId.ColonyCap));
            Assert.AreEqual(30, book.NextCost(UpgradeId.HatchSpeed));
        }

        [Test]
        public void TryBuy_SpendsCostAndRaisesLevel()
        {
            UpgradeBook book = NewBook();
            Wallet wallet = WalletWith(100);

            Assert.IsTrue(book.TryBuy(UpgradeId.WalkSpeed, wallet));

            Assert.AreEqual(80, wallet.Dirt);
            Assert.AreEqual(1, book.Level(UpgradeId.WalkSpeed));
            Assert.AreEqual(29, book.NextCost(UpgradeId.WalkSpeed));
            Assert.AreEqual(2.2f, book.Value(UpgradeId.WalkSpeed), 1e-4f);
            Assert.AreEqual(1, book.TotalLevels);
        }

        [Test]
        public void TryBuy_RaisesBoughtEvent()
        {
            UpgradeBook book = NewBook();
            UpgradeId? bought = null;
            book.Bought += id => bought = id;

            book.TryBuy(UpgradeId.DigSpeed, WalletWith(20));

            Assert.AreEqual(UpgradeId.DigSpeed, bought);
        }

        [Test]
        public void TryBuy_WithoutEnoughDirt_ChangesNothing()
        {
            UpgradeBook book = NewBook();
            Wallet wallet = WalletWith(19);

            Assert.IsFalse(book.TryBuy(UpgradeId.WalkSpeed, wallet));

            Assert.AreEqual(19, wallet.Dirt);
            Assert.AreEqual(0, book.Level(UpgradeId.WalkSpeed));
        }

        [Test]
        public void Cost_RoundsHalfAwayFromZero()
        {
            UpgradeBook book = NewBook();
            book.RestoreLevels(new[] { 0, 0, 0, 2, 0 });

            Assert.AreEqual(113, book.NextCost(UpgradeId.ColonyCap)); // 50 × 1.5² = 112.5
        }

        [Test]
        public void HatchSpeed_IsMultiplicative()
        {
            UpgradeBook book = NewBook();
            book.RestoreLevels(new[] { 0, 0, 0, 0, 12 });

            Assert.AreEqual(12f * Math.Pow(0.88, 12), book.Value(UpgradeId.HatchSpeed), 0.01);
            Assert.AreEqual(2.58f, book.Value(UpgradeId.HatchSpeed), 0.01f);
        }

        [Test]
        public void ValuesAtMax_MatchSpec()
        {
            UpgradeBook book = NewBook();
            book.RestoreLevels(new[] { 15, 15, 8, 20, 12 });

            Assert.AreEqual(5f, book.Value(UpgradeId.WalkSpeed), 1e-4f);
            Assert.AreEqual(8f, book.Value(UpgradeId.DigSpeed), 1e-4f);
            Assert.AreEqual(10f, book.Value(UpgradeId.CarryAmount), 1e-4f);
            Assert.AreEqual(45f, book.Value(UpgradeId.ColonyCap), 1e-4f);
            Assert.AreEqual(70, book.TotalLevels);
        }

        [Test]
        public void AtMaxLevel_CannotBuy_AndNextCostIsMinusOne()
        {
            UpgradeBook book = NewBook();
            book.RestoreLevels(new[] { 0, 0, 8, 0, 0 });
            Wallet wallet = WalletWith(1_000_000_000);

            Assert.IsTrue(book.IsMaxed(UpgradeId.CarryAmount));
            Assert.AreEqual(-1, book.NextCost(UpgradeId.CarryAmount));
            Assert.IsFalse(book.TryBuy(UpgradeId.CarryAmount, wallet));
            Assert.AreEqual(1_000_000_000, wallet.Dirt);
        }

        [Test]
        public void RestoreLevels_ClampsOutOfRange()
        {
            UpgradeBook book = NewBook();

            book.RestoreLevels(new[] { -3, 99, 0, 0, 0 });

            Assert.AreEqual(0, book.Level(UpgradeId.WalkSpeed));
            Assert.AreEqual(15, book.Level(UpgradeId.DigSpeed));
        }

        [Test]
        public void RestoreLevels_WrongLength_Throws()
        {
            Assert.Throws<ArgumentException>(() => NewBook().RestoreLevels(new[] { 1, 2 }));
        }

        [Test]
        public void SnapshotLevels_ReturnsCopy()
        {
            UpgradeBook book = NewBook();
            int[] snapshot = book.SnapshotLevels();
            snapshot[0] = 5;

            Assert.AreEqual(0, book.Level(UpgradeId.WalkSpeed));
        }

        [Test]
        public void Wallet_AddNegative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Wallet().Add(-1));
        }

        [Test]
        public void Wallet_TrySpend_FailsWhenShort_AndRaisesChangedOnSuccess()
        {
            Wallet wallet = WalletWith(10);
            int changes = 0;
            wallet.Changed += _ => changes++;

            Assert.IsFalse(wallet.TrySpend(11));
            Assert.IsTrue(wallet.TrySpend(4));

            Assert.AreEqual(6, wallet.Dirt);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Wallet_Reset_ZeroesDirt()
        {
            Wallet wallet = WalletWith(10);

            wallet.Reset();

            Assert.AreEqual(0, wallet.Dirt);
        }

        [Test]
        public void TierFor_UsesThresholds()
        {
            int[] thresholds = { 0, 10, 30, 55 };

            Assert.AreEqual(0, GrowthTiers.TierFor(0, thresholds));
            Assert.AreEqual(0, GrowthTiers.TierFor(9, thresholds));
            Assert.AreEqual(1, GrowthTiers.TierFor(10, thresholds));
            Assert.AreEqual(1, GrowthTiers.TierFor(29, thresholds));
            Assert.AreEqual(2, GrowthTiers.TierFor(30, thresholds));
            Assert.AreEqual(3, GrowthTiers.TierFor(55, thresholds));
            Assert.AreEqual(3, GrowthTiers.TierFor(70, thresholds));
        }
    }
}
