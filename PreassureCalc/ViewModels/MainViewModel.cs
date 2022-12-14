using PreassureCalc.Models;
using PropertyChanged;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static PreassureCalc.Entity.DbConnection;


namespace PreassureCalc.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel 
    {
        public float depth { get; set; }
        public string numberWell { get; set; }
        public float density { get; set; }
        public WpfPlot graph { get; set; }
        public string nameButtonCalculate { get; set; }
        public int stepCount { get; set; } = 5;
        public double progressBar { get; set; }
        public double maxProgress { get; set; } 
        public string selectedWell { get; set; }

        public Dictionary<string,double[]> Preassure { get; set; }
        public Dictionary<string, double[]> Depth { get; set; }

        object locker = new object();

        private const float g = 9.78f;
        public ObservableCollection<Wells> wells { get; set; }

        private ObservableCollection<Wells> collection = new ObservableCollection<Wells>();

        public MainViewModel()
        {
            nameButtonCalculate = "Рассчитать";
            graph = new WpfPlot();
            Preassure = new Dictionary<string, double[]>();
            Depth = new Dictionary<string, double[]>();

            wells = ObservableToList(db.Wells);
            InitialDataProgressBar();
        }

        /// <summary>
        /// Метод показа графика
        /// </summary>
        /// <param name="well"></param>
        private void GetGraph(Wells well)
        {
            double[] preassure;
            double[] depth;

            Preassure.TryGetValue(well.numberWell, out preassure);
            Depth.TryGetValue(well.numberWell, out depth);

            graph = new WpfPlot();
            graph.Plot.AddScatter(depth, preassure);
            graph.Plot.XLabel("Глубина, м");
            graph.Plot.YLabel("Давление, кПа");

            graph.Refresh();
        }

        /// <summary>
        /// Валидация перед показом графика
        /// </summary>
        /// <param name="well"></param>
        /// <param name="action"></param>
        private void GetGraphValidation(Wells well, Action action)
        {
            if (well == null)
                MessageBox.Show("Выберите скважину");
            else if ((Preassure.Count == 0) || (Depth.Count==0))
                MessageBox.Show("Сначала произведите расчет");
            else
            {
                action();
            }
        }


        /// <summary>
        /// Метод создания массива данных для графика
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        private double[] ArrayCreator(double[] vs, float arg)
        {
            var sum = 0f;
            for (int i = 0; i < stepCount; i++)
            {
                sum = sum + arg / stepCount;
                vs[i] = sum;
            }

            return vs;
        }

        /// <summary>
        /// Выполнение при нажатии кнопки Добавить
        /// </summary>
        public ICommand AddNewWell
        {
            get 
            { 
                return new RelayCommand(obj =>
                {
                    ValidationAddNewWell(AddWell);
                });
            }
        }

        /// <summary>
        /// Выполнение при нажатии кнопки Показать график
        /// </summary>
        public ICommand ShowGraph
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Wells well = (Wells)obj;
                    GetGraphValidation(well, () => GetGraph(well));
                });
            }
        }


        /// <summary>
        /// ВЫполнение при нажатии кнопки Рассчитать
        /// </summary>
        public ICommand CalculatePreassure
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    ValidateCalculation(Calculate);
                });
            }
        }



        /// <summary>
        /// Преобразование коллекции лист в обсербавл
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private ObservableCollection<Wells> ObservableToList(DbSet<Wells> list)
        {
            if (list.FirstOrDefault() != null)
            {
                collection.Clear();
                foreach (var item in list.ToList())
                {
                    collection.Add(item);
                }
                return collection;
            }
            else
                return new ObservableCollection<Wells>();
        }

        /// <summary>
        /// Начальные данные прогресс бара
        /// </summary>
        private void InitialDataProgressBar()
        {
            progressBar = 0;
            maxProgress = (wells.Count == 0 ? 1 : wells.Count);
        }

        private void ValidateCalculation(Action action)
        {
            if (stepCount <= 0)
                MessageBox.Show("Кол-во шагов разбиения должно быть больше 0");
            else if (wells.Count != 0)
                action();
        }

        /// <summary>
        /// Вычисление давления и запись в бд
        /// </summary>
        private async void Calculate()
        {
            ChangeButtonCalculate();
            Preassure.Clear();
            Depth.Clear();
            await Task.Run(() =>
            {
                Parallel.For(0, wells.Count, new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                (i, state) =>
                {
                    if (nameButtonCalculate == "Рассчитать")
                    {
                        state.Break();
                    }
                    else
                    {
                        if ((wells[i].calculatedDepth <= wells[i].depth) && (wells[i].calculatedDepth >= 0))
                        {
                            wells[i].preassure = wells[i].density * g * wells[i].calculatedDepth / 1000;
                        }

                        else if (wells[i].calculatedDepth > wells[i].depth)
                        {
                            wells[i].calculatedDepth = wells[i].depth;
                            wells[i].preassure = wells[i].density * g * wells[i].calculatedDepth / 1000;
                        }

                        else
                        {
                            wells[i].preassure = 0;
                            wells[i].calculatedDepth = 0;
                        }
                        
                            double[] Preassure = new double[stepCount];
                            Preassure = ArrayCreator(Preassure, wells[i].preassure);
                            this.Preassure.Add(wells[i].numberWell, Preassure);
                       
                            double[] Depth = new double[stepCount];
                            Depth = ArrayCreator(Depth, wells[i].calculatedDepth);
                            this.Depth.Add(wells[i].numberWell, Depth);

                        lock (locker)
                        {
                            progressBar++;
                        }
                        Thread.Sleep(300);
                    }
                });
            });

            if (progressBar >= maxProgress)
                ChangeButtonCalculate();

            InitialDataProgressBar();

            await db.SaveChangesAsync();

            wells = ObservableToList(db.Wells);
        }

        /// <summary>
        /// Изменение название кнопки рассчитать
        /// </summary>
        private void ChangeButtonCalculate()
        {
            nameButtonCalculate = (nameButtonCalculate == "Рассчитать" ? "Отменить" : "Рассчитать");
        }


        /// <summary>
        /// Валидация данных при добавлении новой скважины
        /// </summary>
        /// <param name="action"></param>
        private void ValidationAddNewWell(Action action)
        {
            var well = db.Wells.FirstOrDefault(x => x.numberWell == numberWell);
            if ((density < 730) || (density > 1100))
                MessageBox.Show("Плотность должна быть от 730 до 1100");
            else if (depth <= 0)
                MessageBox.Show("Глубина должна быть больше 0");
            else if (string.IsNullOrEmpty(numberWell))
                MessageBox.Show("Номер скважины не может быть пустым");
            else if (well != null)
            {
                MessageBox.Show($"Скважина с номером {numberWell} уже существует");
            }
            else
                action();
        }

        /// <summary>
        /// Сброс данных после добавления скважины
        /// </summary>
        private void NewWellDataReset()
        {
            numberWell = "";
            depth = 0;
            density = 0;
        }


        /// <summary>
        /// Запись новой скважины в базу данных
        /// </summary>
        private async void AddWell()
        {
            var well = new Wells()
            {
                depth = depth,
                numberWell = numberWell,
                density = density
            };

            wells.Add(well);
            db.Wells.Add(well);
            await db.SaveChangesAsync();
            NewWellDataReset();
        }


    }
}
