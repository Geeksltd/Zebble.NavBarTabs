namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Tabs : Stack
    {
        public Tabs() : base(RepeatDirection.Horizontal) { }

        public async Task<Tab<TPage>> AddTab<TPage>(string text, string iconPath = null) where TPage : Page, new()
        {
            var tab = new Tab<TPage>();
            tab.Label.Text = text;
            if (iconPath.HasValue()) tab.Icon.Path = iconPath;

            await Add(tab);
            return tab;
        }

        public override async Task OnPreRender()
        {
            await base.OnPreRender();

            await HighlightSelectedTab();

            Nav.Navigated.Handle(HighlightSelectedTab);
        }

        Task HighlightSelectedTab() => HighlightSelectedTab(Nav.CurrentPage);

        public Task HighlightSelectedTab(Page activePage)
        {
            var bySetting = activePage?.Data<string>("CurrentTab");
            if (bySetting.HasValue())
            {
                GetTabs().FirstOrDefault(x => x.Label.Text == bySetting).Perform(x => x.Selected = true);
                return Task.CompletedTask;
            }

            var currentNavPath = Nav.Stack.Select(s => s.Page).Concat(activePage?.Page).ExceptNull()
                           .Select(c => c.GetType()).Reverse().ToArray();

            var tabsInNavHistory = GetTabs()
                .Where(tab => tab.TargetPageType != null)
                .Where(tab => currentNavPath.Contains(tab.TargetPageType))
                .Select(x => new { Tab = x, Distance = currentNavPath.IndexOf(p => p == x.TargetPageType) })
                .ToArray();

            if (tabsInNavHistory.Any())
            {
                var selected = tabsInNavHistory.WithMin(x => x.Distance)?.Tab;
                selected.Perform(t => t.Selected = true);
            }

            return Task.CompletedTask;
        }

        public class Tab<TPage> : Tab where TPage : Page, new()
        {
            public Tab() { TargetPageType = typeof(TPage); }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();
                this.On(x => x.Tapped, HandleTapped);
            }

            Task HandleTapped()
            {
                Selected = true;
                return Nav.Go<TPage>(Transition);
            }
        }

        public IEnumerable<Tab> GetTabs() => AllDescendents().OfType<Tab>();

        public class Tab : Canvas
        {
            bool selected;
            public readonly AsyncEvent SelectedChanged = new AsyncEvent();
            public readonly Stack Stack = new Stack();
            public readonly ImageView Icon = new ImageView { Id = "Icon" };
            public readonly TextView Label = new TextView { Id = "Label" };

            public Tab(string text = "") { Label.Text = text; }

            public PageTransition Transition { get; set; } = PageTransition.None;

            public virtual Type TargetPageType { get; set; }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();
                await Add(Stack);
                await Stack.Add(Icon);
                await Stack.Add(Label);
            }

            protected override string GetStringSpecifier() => Label.Text.Or(Icon.Path);

            public bool Selected
            {
                get => selected;
                set => SetSelected(value).RunInParallel();
            }

            /// <summary>
            /// This is the same as setting the Selected property, but allows you to await it.
            /// </summary>
            public async Task SetSelected(bool value)
            {
                var tasks = new List<Task>();

                if (value)
                {
                    foreach (var tab in FindParent<Tabs>().GetTabs().Except(this))
                    {
                        tab.selected = false;
                        await tab.UnsetPseudoCssState("active");
                        tasks.Add(tab.SelectedChanged.Raise());
                    }

                    await SetPseudoCssState("active");
                }
                else await UnsetPseudoCssState("active");

                selected = value;
                tasks.Add(SelectedChanged.Raise());
            }

            public override void Dispose()
            {
                SelectedChanged?.Dispose();
                base.Dispose();
            }
        }
    }
}