using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using MazakTransfer.Util;

namespace MazakTransfer
{
    /// <summary>
    /// Interaction logic for FileList.xaml
    /// </summary>
    public partial class FileList : UserControl
    {
        public delegate void FileSelectionChangedEventHandler(object sender, EventArgs e);

        public event FileSelectionChangedEventHandler FileSelectionChanged;

        private GridViewColumnHeader _lastHeaderClicked;
        private SortAdorner _lastSortAdorner;
        private string _filterString;
        private string _filePath;
        private FileSearcher _fileSearcher;
        private Timer _timer;

        public List<FileData> FileDataCollection { get; set; }

        private ListCollectionView FileCollectionDataView
        {
            get { return (ListCollectionView)CollectionViewSource.GetDefaultView(FileDataCollection); }
        }

        //public static DependencyProperty FilePathDependencyProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(FileList), new PropertyMetadata(string.Empty));
        //public string FilePath
        //{
        //    get { return (string)GetValue(FilePathDependencyProperty); }

        //    set
        //    {
        //        SetValue(FilePathDependencyProperty, value);
        //        InitializeFileList(value);
        //    }
        //}

        public string FilePath
        {
            set
            {
                _filePath = value;
                SearchFileList();
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                //Alussa FileCollectionDataView voi olla myös null
                if (FileCollectionDataView == null)
                    return;

                FileCollectionDataView.Refresh();

                //Jos rajatun listan koko on yksi, tehdään automaattinen valinta
                if (FileCollectionDataView.Count == 1)
                {
                    //var onlyItem = FileCollectionDataView.OfType<FileData>().First();
                    //listViewFiles.SelectedItem = onlyItem;
                }
            }
        }

        public FileData SelectedItem
        {
            get { return ListViewFiles.SelectedItem as FileData; }
            set { ListViewFiles.SelectedItem = value; }
        }

        public FileList()
        {
            InitializeComponent();
            //Luodaan alkuu tyjä collection, jota muokataan myöhemmin
            FileDataCollection = new List<FileData>();

            //Asetetaan filtteröinnin tekevä metodi
            FileCollectionDataView.Filter = FileNameFilter;

            //Luodaan FileSearcher
            _fileSearcher = new FileSearcher(Dispatcher);

            //Alustetaan ja käynnistetään ajastin, jos aika on positiivinen
            if (Properties.Settings.Default.FileListUpdateInterval > 0)
            {
                _timer = new Timer(Properties.Settings.Default.FileListUpdateInterval*1000);
                //Tehdään tiedostojen haku. täytyy tehdä Dispatcherin kautta koska timer on eri säikeessä
                _timer.Elapsed += (sender, args) => Dispatcher.Invoke(new Action(Refresh));
                _timer.Enabled = true;
            }
        }

        //Hakee tiedostolistan, tätä kutsuttaan aina kun haku tehdään
        private void SearchFileList()
        {
            //Tehdään asynkronisesti tiedostojen haku
            const string searchPattern = "*.*";
            if (!String.IsNullOrWhiteSpace(_filePath))
            {
                //Progressbar päälle
                FileListProgressBar.Visibility = Visibility.Visible;

                //Suoritetaan haku. Metodi tekee vain yhden haun kerrallaan. Jos aikaisempi haku on kesken, jälkimmäinen ei tee mitään
                _fileSearcher.SearchFileList(_filePath, searchPattern, SearchDone, SearchDoneWithError);
            }
        }

        private void SearchDoneWithError(Exception exception)
        {
            //Siivotaan vanhat tulokset
            FileDataCollection.Clear();
            //Refreshataan näkymä
            FileCollectionDataView.Refresh();
            //Progressbar pois
            FileListProgressBar.Visibility = Visibility.Hidden;

            //File search ends with an exception. Throw that exception
            throw exception;
        }

        //Tätä kutsutaan kun haku on valmis
        public void SearchDone(IEnumerable<FileData> result)
        {
            //Haetaan valittu alkio talteen
            var selectedItem = SelectedItem;

            //Siivotaan vanhat tulokset
            FileDataCollection.Clear();
            //Lisätään koko hakutulos
            FileDataCollection.AddRange(result);

            //Jos jotain oli valittu, valitaan sama myös päivitetyssä listassa
            if (selectedItem != null)
            {
                var itemToSelect = FileDataCollection.Find(f => f.FileName == selectedItem.FileName);
                if (itemToSelect != null)
                {
                    SelectedItem = itemToSelect;
                }
            }

            //Refreshataan näkymä
            FileCollectionDataView.Refresh();

            //Progressbar pois
            FileListProgressBar.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Refreshaa näkymän, hakee tiedostolistauksen uudelleen
        /// </summary>
        public void Refresh()
        {
            SearchFileList();
        }

        private void listViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ListViewFiles.SelectedItem as FileData;

            var handler = FileSelectionChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            SortAndSetAdorner(headerClicked);
        }

        private void SortAndSetAdorner(GridViewColumnHeader sortByHeader)
        {
            //Sorttisuunta on oletuksena nouseva
            ListSortDirection direction = ListSortDirection.Ascending;

            //Padding header on viimeisenä
            if (sortByHeader != null && sortByHeader.Role != GridViewColumnHeaderRole.Padding)
            {
                //Jos on olemassa jokin vanha sorttaus, poistetaan kuva
                if (_lastHeaderClicked != null)
                {
                    AdornerLayer.GetAdornerLayer(_lastHeaderClicked).Remove(_lastSortAdorner);
                }

                //Jos painetaan samaa kolumn otsikkoa kuin viimeksi, sorttisuunta kääntyy
                if (Equals(sortByHeader, _lastHeaderClicked))
                {
                    direction = _lastSortAdorner.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }

                //Sortataan, sidotun propertyn mukaan
                var sortingProperty = ((Binding)sortByHeader.Column.DisplayMemberBinding).Path.Path;
                Sort(sortingProperty, direction);

                //Tallentaan kuva ja viimeisin sorttikolumni
                _lastSortAdorner = new SortAdorner(sortByHeader, direction);
                AdornerLayer.GetAdornerLayer(sortByHeader).Add(_lastSortAdorner);
                _lastHeaderClicked = sortByHeader;
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            FileCollectionDataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            FileCollectionDataView.SortDescriptions.Add(sd);

            //Käytetään NaturalStringComparer luokkaa jos ollaan sorttaamassa nimen mukaan. Tällöin tiedostot menevät numerojärjestykseen.
            if (sortBy == ((Binding)FileNameColumnHeader.Column.DisplayMemberBinding).Path.Path)
            {
                FileCollectionDataView.CustomSort = new NaturalStringComparer(sortBy, direction);
            }
            
            FileCollectionDataView.Refresh();
        }

        private bool FileNameFilter(Object item)
        {
            FileData fileData = (FileData) item;
            if (String.IsNullOrWhiteSpace(_filterString))
            {
                return true;
            }

            return fileData.FileNameWithoutExtension.Contains(_filterString);
        }

        private void ListViewFiles_Loaded(object sender, RoutedEventArgs e)
        {
            //Sortataan aluksi nimen mukaan, luonnolliseen järjestykseen. Pitää olla loaded eventin jälkeen, jos laittaa konstruktoriin, tuli poikkeus SortAndSetAdorner() metodissa
            SortAndSetAdorner(FileNameColumnHeader);
        }
    }
}
