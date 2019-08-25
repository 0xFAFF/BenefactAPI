using BenefactAPI;
using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicate;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenefactAPITests
{
    [TestClass]
    public class CardTests : BaseTest
    {
        [TestMethod]
        public async Task ArchiveCardDeveloperSucceeds()
        {
            var cardId = (await services.DoWithDB(db => db.Cards.FirstOrDefaultAsync())).Id;
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Developer);
            var firstResult = await Post<None, CardArchiveRequest>("cards/archive",
                new CardArchiveRequest() { CardId = cardId });
            var archive = (await services.DoWithDB(db => db.Cards
            .Where(c => c.Id == cardId)
            .FirstOrDefaultAsync())).Archived;
            Assert.IsTrue(archive);
        }
        [TestMethod]
        public async Task ArchiveCardOwnerSucceeds()
        {
            var cardId = (await services.DoWithDB(db => db.Cards.FirstOrDefaultAsync())).Id;
            user = Auth.CurrentUser = await GetUser("faff@faff.faff", Privilege.Read);
            var firstResult = await Post<None, CardArchiveRequest>("cards/archive",
                new CardArchiveRequest() { CardId = cardId });
            var archive = (await services.DoWithDB(db => db.Cards
            .Where(c => c.Id == cardId)
            .FirstOrDefaultAsync())).Archived;
            Assert.IsTrue(archive);
        }
        [TestMethod]
        public async Task ArchiveCardNotOwnerFails()
        {
            var cardId = (await services.DoWithDB(db => db.Cards.FirstOrDefaultAsync())).Id;
            user = Auth.CurrentUser = await GetUser("a@a.a", Privilege.Read);
            var error = Assert.ThrowsExceptionAsync<HTTPError>(
                () => Post<None, CardArchiveRequest>("cards/archive",
                new CardArchiveRequest() { CardId = cardId }));
        }
        [TestMethod]
        public async Task UnarchiveSucceeds()
        {
            var cardId = (await services.DoWithDB(async db =>
            {
                var card = await db.Cards.FirstOrDefaultAsync();
                card.Archived = true;
                return card;
            })).Id;
            user = Auth.CurrentUser = await GetUser("faff@faff.faff", Privilege.Read);
            var firstResult = await Post<None, CardArchiveRequest>("cards/archive",
                new CardArchiveRequest() { CardId = cardId, Archive = false });
            var archive = (await services.DoWithDB(db => db.Cards
            .Where(c => c.Id == cardId)
            .FirstOrDefaultAsync())).Archived;
            Assert.IsFalse(archive);
        }
        [TestMethod]
        public async Task CardStateUpdates()
        {
            user = Auth.CurrentUser = await GetUser("faff@faff.faff", Privilege.Admin);
            var query = new CardQuery()
            {
                Groups = new Dictionary<string, List<CardQueryTerm>>()
                    {
                        { "Done", new List<CardQueryTerm>()
                        {
                            new CardQueryTerm() { State = CardState.InProgress }
                        } }
                    }
            };
            var response = await Post<BoardResponse, CardQuery>("/", query);
            var cards = response.Cards.First().Value;
            var doneColumn = response.Columns.First(c => c.State == CardState.Complete);
            Assert.IsTrue(cards.Any());
            await Post<None, CardData>("cards/update", new CardData()
            {
                ColumnId = doneColumn.Id,
                Id = cards.First().Id,
            });
            var response2 = await Post<BoardResponse, CardQuery>("/", query);
            Assert.IsTrue(response2.Cards.Values.First().Count < cards.Count);
        }
    }
}
