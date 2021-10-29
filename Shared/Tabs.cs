namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;

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
            HighlightSelectedTab();
            Nav.Navigated.FullEvent += x => HighlightSelectedTab(x.To);
        }

        void HighlightSelectedTab() => HighlightSelectedTab(Nav.CurrentPage);

        public void HighlightSelectedTab(Page activePage)
        {
            if (activePage is PopUp) return;

            var tabs = GetTabs();

            var bySetting = activePage?.Data<string>("CurrentTab");
            if (bySetting.HasValue())
            {
                tabs.FirstOrDefault(x => x.Label.Text == bySetting).Perform(x => x.Selected = true);

                tabs
                    .Where(x => x.Label.Text != bySetting && x.Selected)
                    .Do(async x => await x.SetSelected(value: false, disableEvent: true));

                return;
            }
            else
            {
                tabs
                    .Where(x => x.TargetPageType != activePage?.GetType() && x.Selected)
                    .Do(async x => await x.SetSelected(value: false, disableEvent: true));
            }

            var currentNavPath = Nav.Stack.ToArray().Select(s => s.Page).Concat(activePage?.Page).ExceptNull()
                .Select(c => c.GetType()).Reverse().ToArray();

            var selected = tabs
                .Where(tab => tab.TargetPageType != null)
                .Where(tab => currentNavPath.Contains(tab.TargetPageType))
                .Select(x => new { Tab = x, Distance = currentNavPath.IndexOf(p => p == x.TargetPageType) })
                .WithMin(x => x.Distance)?.Tab;

            selected?.Set(t => t.Selected = true);
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
                if (UseNavForward) return Nav.Forward<TPage>(navParams: NavParams, transition: Transition);
                return Nav.Go<TPage>(navParams: NavParams, transition: Transition);
            }
        }

        public Tab[] GetTabs() => AllDescendents().OfType<Tab>().ToArray();

        public class Tab : Canvas
        {
            bool selected;
            /// <summary>
            /// When set true uses the Nav.Forward() for navigating. Otherwise it will be a Nav.Go()
            /// </summary>
            public bool UseNavForward;
            public object NavParams;
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
            public Task SetSelected(bool value) => SetSelected(value, disableEvent: false);

            internal async Task SetSelected(bool value, bool disableEvent)
            {
                var tasks = new List<Task>();

                if (value)
                {
                    foreach (var tab in (FindParent<Tabs>()?.GetTabs()).OrEmpty().Except(this))
                    {
                        tab.selected = false;
                        await tab.UnsetPseudoCssState("active");
                        if (!disableEvent)
                            tasks.Add(tab.SelectedChanged.Raise());
                    }

                    await SetPseudoCssState("active");
                }
                else await UnsetPseudoCssState("active");

                selected = value;
                if (!disableEvent)
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