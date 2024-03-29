namespace Zebble
{
    using System;
    using System.Threading.Tasks;

    public class NavigationBar : Stack
    {
        public readonly TextView Title;

        public readonly Stack Left = new Stack(RepeatDirection.Horizontal)
            .Id("Left")
            .Size(50.Percent(), 100.Percent());

        public readonly Stack Right = new Stack(RepeatDirection.Horizontal)
            .Id("Right")
            .Set(x => x.HorizontalAlignment = HorizontalAlignment.Right)
            .Size(50.Percent(), 100.Percent());

        public NavigationBar()
        {
            Direction = RepeatDirection.Horizontal;
            Title = CreateTitle();
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();
            await Add(Title);
            await Add(Left);
            await Add(Right);
        }

        protected virtual TextView CreateTitle() => new();

        public Task<TView> AddButton<TView>(ButtonLocation location, TView button) where TView : View
        {
            if (location == ButtonLocation.Left) return Left.Add(button);
            if (location == ButtonLocation.Right) return Right.Add(button);
            else throw new NotSupportedException(location + " is not supported?!");
        }
    }
}