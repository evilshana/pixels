using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace pixels
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        const int SIZE = 20;
        const int ROWS = 15;
        const int COLS = 20;
        protected override void OnActivated(EventArgs e)
        {
            SetupGrid();
            base.OnActivated(e);
        }

        void SetupGrid()
        {
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    var rect = new Rectangle()
                    {
                        Width = SIZE,
                        Height = SIZE,
                        Stroke = Brushes.Gray,
                        StrokeDashArray = new DoubleCollection {1}
                    };
                    rect.SetValue(Canvas.LeftProperty, (double) c*SIZE);
                    rect.SetValue(Canvas.TopProperty, (double) r*SIZE);
                    canvas.Children.Add(rect);
                }
            }
        }

        Random random = new Random();
        async void FillGrid_Task(Brush brush)
        {
            for (var i = 0; i < 10; i++)
            {
                var pos = await GetNextPos();
                ((Rectangle)canvas.Children[pos]).Fill = brush;
                log.Children.Add(GetTextBlock(pos.ToString()));
            }
            log.Children.Add(GetTextBlock("Completed"));
        }

        void FillGrid_Enumerable(Brush brush)
        {
            foreach (var pos in GetPositions_Task())
            {
                ((Rectangle)canvas.Children[pos]).Fill = brush;
                log.Children.Add(GetTextBlock(pos.ToString()));
            }
            log.Children.Add(GetTextBlock("Completed"));
        }

        void FillGrid_Obs(Brush brush)
        {
            var list = GetPositions();

            // draw the pixels
            list.Subscribe(pos => ((Rectangle)canvas.Children[pos]).Fill = brush);

            // log the positions
            list
                .Select(x => x.ToString())
                .Subscribe(pos => log.Children.Add(GetTextBlock(pos)), () => {
                    log.Children.Add(GetTextBlock("Completed"));
                    // replay the values to the log again
                    list
                        .Select(x => x.ToString())
                        .Subscribe(pos => log.Children.Add(GetTextBlock(pos)));
                });
            
        }

        TextBlock GetTextBlock(string txt)
        {
            var block = new TextBlock() { Text = txt };
            block.SetValue(DockPanel.DockProperty, Dock.Top);
            return block;
        }

        IEnumerable<int> GetPositions_Task()
        {
            for (var i = 0; i < 10; i++)
                yield return GetNextPos().Result;
        }


        IObservable<int> GetPositions()
        {
            return Observable.Create<int>(async o =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var pos = await GetNextPos();
                    o.OnNext(pos);
                }
                o.OnCompleted();
                return () => { };
            }).Replay().RefCount();
        }

        Task<int> GetNextPos()
        {
            return Task.Run(() =>
            {
                Thread.Sleep(500);
                return random.Next(ROWS * COLS - 1);
            });
        }

        void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
            FillGrid_Task(brush);
        }

        void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
            FillGrid_Enumerable(brush);
        }

        void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
            FillGrid_Obs(brush);
        }
    }
}
