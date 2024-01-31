using AutoFixture;
using FluentAssertions;
using GhostfolioSidekick.Model;
using GhostfolioSidekick.Model.Accounts;
using GhostfolioSidekick.Model.Activities;
using GhostfolioSidekick.Parsers.Nexo;
using GhostfolioSidekick.Parsers.UnitTests;

namespace GhostfolioSidekick.Parsers.UnitTests.Nexo
{
	public class NexoParserTests
	{
		private NexoParser parser;
		private Account account;
		private TestHoldingsCollection holdingsAndAccountsCollection;

		public NexoParserTests()
		{
			parser = new NexoParser();

			var fixture = new Fixture();
			account = fixture
				.Build<Account>()
				.With(x => x.Balance, new Balance(new Money(Currency.EUR, 0)))
				.Create();
			holdingsAndAccountsCollection = new TestHoldingsCollection(account);
		}

		[Fact]
		public async Task CanParseActivities_TestFiles_True()
		{
			// Arrange
			foreach (var file in Directory.GetFiles("./TestFiles/Nexo/", "*.csv", SearchOption.AllDirectories))
			{
				// Act
				var canParse = await parser.CanParseActivities(file);

				// Assert
				canParse.Should().BeTrue($"File {file}  cannot be parsed");
			}
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleDeposit_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/CashTransactions/single_deposit.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
				[
					PartialActivity.CreateCashDeposit(
						Currency.EUR,
						new DateTime(2023, 08, 25, 14, 44, 44, DateTimeKind.Utc),
						150,
						"NXTM6EtqQukSs")
				]);
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleBuy_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/BuyOrders/single_buy.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
				[
					PartialActivity.CreateBuy(
						Currency.EUR,
						new DateTime(2023, 08, 25, 14, 44, 46, DateTimeKind.Utc),
						[PartialSymbolIdentifier.CreateCrypto("USDC")],
						150,
						0.9264700400075475907102725806M,
						"NXTyPxhiopNL3")
				]);
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleConvert_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/BuyOrders/single_convert.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
					PartialActivity.CreateAssetConvert(
						new DateTime(2023, 10, 08, 19, 54, 20, DateTimeKind.Utc),
						[PartialSymbolIdentifier.CreateCrypto("USDC")],
						200M,
						[PartialSymbolIdentifier.CreateCrypto("BTC")],
						0.00716057M,
						"NXTVDI4DJFWqB63pTcCuTpgc")
				);
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleCashbackCrypto_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/Specials/single_cashback_crypto.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
				[
					PartialActivity.CreateGift(
						new DateTime(2023, 10, 12, 10, 44, 32, DateTimeKind.Utc),
						[PartialSymbolIdentifier.CreateCrypto("BTC")],
						0.00000040M,
						"NXT2yQdOutpLLE1Lz51xXt6uW")
				]);
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleCashbackFiat_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/Specials/single_cashback_fiat.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
				[
					PartialActivity.CreateGift(
						Currency.EUR,
						new DateTime(2023, 10, 8, 20, 5, 12, DateTimeKind.Utc),
						0.06548358M,
						"NXT6asbYnZqniNoTss0nyuIxM")
				]);
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleReferralBonusPending_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/Specials/single_referralbonus_pending.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEmpty();
		}

		[Fact]
		public async Task ConvertActivitiesForAccount_SingleReferralBonusApproved_Converted()
		{
			// Arrange

			// Act
			await parser.ParseActivities("./TestFiles/Nexo/Specials/single_referralbonus_approved.csv", holdingsAndAccountsCollection, account.Name);

			// Assert
			holdingsAndAccountsCollection.PartialActivities.Should().BeEquivalentTo(
				[
					PartialActivity.CreateGift(
						new DateTime(2023, 08, 25, 16, 43, 55, DateTimeKind.Utc),
						[PartialSymbolIdentifier.CreateCrypto("BTC")],
						0.00096332M,
						"NXTk6FBYyxOqH")
				]);
		}
	}
}