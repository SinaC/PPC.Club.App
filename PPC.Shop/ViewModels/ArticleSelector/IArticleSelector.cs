using System;
using PPC.Data.Contracts;

namespace PPC.Shop.ViewModels.ArticleSelector
{
    public class ArticleSelectedEventArgs : EventArgs
    {
        public Article Article { get; private set; }
        public int Quantity { get; private set; }

        public ArticleSelectedEventArgs(Article article, int quantity)
        {
            Article = article;
            Quantity = quantity;
        }
    }

    public interface IArticleSelector
    {
        event EventHandler<ArticleSelectedEventArgs> ArticleSelected;

        void GotFocus();
    }
}
