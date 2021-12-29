using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ORMAPCancelledNumbers
{
    internal class CancelledNumbersDockpaneViewModel : DockPane
    {

        #region Constructors
        protected CancelledNumbersDockpaneViewModel()
        {
            SearchCommand = new RelayCommand(SearchCommandExecute, CanSearchCommandExecute);
            UpCommand = new RelayCommand(MoveCommandExecute, CanMoveCommandExecute);
            DownCommand = new RelayCommand(MoveCommandExecute, CanMoveCommandExecute);
            TopCommand = new RelayCommand(MoveCommandExecute, CanMoveCommandExecute);
            BottomCommand = new RelayCommand(MoveCommandExecute, CanMoveCommandExecute);
            AddCommand = new RelayCommand(AddCommandExecuteAsync, CanAddCommandExecute);
            DeleteCommand = new RelayCommand(DeleteCommandExecuteAsync, HasSelection);
            UpdateCommand = new RelayCommand(UpdateCommandExecute, CanUpdateCommandExecute);
            CancelCommand = new RelayCommand(CancelCommandExecuteAsync, CanCancelCommandExecute);
            CloseModalCommand = new RelayCommand(CloseModalCommandExecute, CanCloseModalCommandExecute);
            SearchCommand = new RelayCommand(SearchCommandExecute);
            
            this.PropertyChanged += OnFeatureDataChanged;
            MapViewInitializedEvent.Subscribe(OnMapViewInitializedEvent);
            StandaloneTablesRemovedEvent.Subscribe(OnStandaloneTablesRemovedEvent);
        }

        #endregion Constructors


        #region EventHandlers

        private async void OnMapViewInitializedEvent(MapViewEventArgs obj)
        {
            if (obj.MapView.IsReady)
            {
                _activeMapView = obj.MapView;

                if (await TestPrereqsAsync(false))
                {
                    GetMapNumbers();
                }
                else
                {
                    this.Hide();
                }
            }
        }

        private async void OnStandaloneTablesRemovedEvent(StandaloneTableEventArgs obj)
        {
            if (obj.Tables.Any(a => a.Name.ToUpper() == "CANCELLEDNUMBERS"))
            {
                if (!await TestPrereqsAsync(true))
                {
                    MapNumbers = null;
                    this.Hide();
                }
            }
        }

        protected override async void OnShow(bool isVisible)
        {

            if (isVisible && MapNumbers is null && _showClicked)
            {
                _showClicked = false;

                if (_activeMapView is null && MapView.Active != null) _activeMapView = MapView.Active;

                if (await TestPrereqsAsync(true))
                {
                    GetMapNumbers();
                }
                else
                {
                    this.Hide();
                }

            }
        }

        private void OnFeatureDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureData")
            {
                _featuresChanged = FeatureData != null ? true : false;
            }
        }

        #endregion EventHandlers


        #region PrivateProperties

        private const string _dockPaneID = "ORMAPCancelledNumbers_CancelledNumbersDockpane";
        internal static bool _showClicked = false;
        private bool _modalResult { get; set; }
        private List<long> _deleteOIDs { get; set; }
        private bool _featuresChanged { get; set; } = false;
        private MapView _activeMapView { get; set; }

        #endregion PrivateProperties


        #region PublicProperties

        public ICommand SearchCommand { get; private set; }
        public ICommand UpCommand { get; private set; }
        public ICommand DownCommand { get; private set; }
        public ICommand TopCommand { get; private set; }
        public ICommand BottomCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand CloseModalCommand { get; private set; }
        public ICommand MapNumberChangedCommand { get; private set; }




        private List<string> _mapNumbers;
        public List<string> MapNumbers
        {
            get
            {
                return _mapNumbers;
            }
            private set
            {
                SetProperty(ref _mapNumbers, value, () => MapNumbers);
                NotifyPropertyChanged(new PropertyChangedEventArgs("AllowTextSearch"));
            }
        }

        public bool AllowTextSearch
        {
            get
            {
                return !IsModalOpen && MapNumbers != null;
            }
        }

        private bool _isModalOpen = false;
        public bool IsModalOpen
        {
            get
            {
                return _isModalOpen;
            }
            set
            {
                SetProperty(ref _isModalOpen, value, () => IsModalOpen);
                NotifyPropertyChanged(new PropertyChangedEventArgs("AllowTextSearch"));
            }
        }

        private string _modalMessage;
        public string ModalMessage
        {
            get
            {
                return _modalMessage;
            }
            set
            {
                SetProperty(ref _modalMessage, value, () => ModalMessage);
            }
        }


        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (MapNumbers.Contains(value)) MapNumber = value;
                SetProperty(ref _searchText, value, () => SearchText);
            }
        }


        private string _mapNumber;
        public string MapNumber
        {
            get { return _mapNumber; }
            set
            {
                SetProperty(ref _mapNumber, value, () => MapNumber);
            }
        }

        private string _newCancelledNum;
        public string NewCancelledNum
        {
            get { return _newCancelledNum; }
            set
            {
                SetProperty(ref _newCancelledNum, value, () => NewCancelledNum);
            }
        }

        private IList _selectedItemsList = new ArrayList();
        public IList SelectedItemsList
        {
            get { return _selectedItemsList; }
            set
            {
                SetProperty(ref _selectedItemsList, value, () => SelectedItemsList);
            }
        }


        private ObservableCollection<CancelledNumsData> _featureData;
        public ObservableCollection<CancelledNumsData> FeatureData
        {
            get { return _featureData; }
            set
            {
                SetProperty(ref _featureData, value, () => FeatureData);
            }
        }


        private string _heading = "Cancelled Numbers Manager";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        #endregion PublicProperties


        #region RelayCommandMethods

        private async Task SearchCommandExecute()
        {

            if (_featuresChanged)
            {
                await ShowModalAsync("Your changes will be lost.  Do you want to save your changes before proceeding?");
                if (_modalResult)
                {
                    await UpdateCancelledNumbersAsync();
                    ClearDockPane(false);
                    return;
                }
                else
                {
                    MapNumber = null; //Clear the MapNumber so the Taxlot list clears
                }
            }

            if (!MapNumbers.Contains(MapNumber))
            {
                FeatureData = null;
                return;
            }


            await QueuedTask.Run(() =>
            {
                try
                {
                    _deleteOIDs = new List<long>();
                    var queryFilter = new QueryFilter();
                    queryFilter.WhereClause = $@"MapNumber = '{MapNumber}'";

                    var list = new List<CancelledNumsData>();
                    var table = _activeMapView.Map.FindStandaloneTables("CancelledNumbers").First() as StandaloneTable;
                    using (RowCursor rowCursor = table.Search(queryFilter))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row current = rowCursor.Current)
                            {
                                var MapNumber = Convert.ToString(current["MapNumber"]);
                                var Taxlot = Convert.ToString(current["Taxlot"]);
                                var SortOrder = Convert.ToInt32(current["SortOrder"]);
                                var ObjectId = Convert.ToInt32(current["OBJECTID"]);
                                list.Add(new CancelledNumsData
                                {
                                    MapNumber = MapNumber,
                                    Taxlot = Taxlot,
                                    SortOrder = SortOrder,
                                    ObjectId = ObjectId
                                });
                            }
                        }
                    }
                    FeatureData = new ObservableCollection<CancelledNumsData>(list.OrderBy(s => s.SortOrder));
                    _featuresChanged = false;
                }
                catch (Exception ex)
                {
                    ShowErrors($@"Query error: {ex}");
                }
            });
        }


        private bool CanSearchCommandExecute()
        {
            return !string.IsNullOrEmpty(MapNumber);
        }


        private async void UpdateCommandExecute()
        {
            await UpdateCancelledNumbersAsync();
            ClearDockPane(true);
        }


        private bool CanUpdateCommandExecute()
        {
            return _featuresChanged;
        }


        private async Task CancelCommandExecuteAsync()
        {
            if (_featuresChanged)
            {
                await ShowModalAsync("Your changes will be lost.  Do you want to save your changes before proceeding?");
                if (_modalResult)
                {
                    await UpdateCancelledNumbersAsync();
                }
            }
            ClearDockPane(true);
        }

        private bool CanCancelCommandExecute()
        {
            return MapNumbers != null;
        }


        private void MoveCommandExecute(object obj)
        {
            var dir = obj.ToString();
            var selectedItems = SelectedItemsList.Cast<CancelledNumsData>().ToList();
            var sortedItems = selectedItems.OrderBy(o => FeatureData.IndexOf(o)).ToList();
            if (dir == "++") sortedItems.Reverse(); // reverse so they stay in order

            foreach (var item in sortedItems)
            {
                int index = FeatureData.IndexOf(item);
                int newIndex = index;
                if (dir == "--") newIndex = FeatureData.Count() - 1;
                if (dir == "-") newIndex = index + sortedItems.Count();
                if (dir == "+") newIndex = index - 1;
                if (dir == "++") newIndex = 0;
                FeatureData.Move(index, newIndex);
            }
            NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
        }


        private async Task DeleteCommandExecuteAsync()
        {
            var selectedItems = SelectedItemsList.Cast<CancelledNumsData>().ToList();
            if (selectedItems.Count() > 1)
            {
                await ShowModalAsync("Are you sure you want to delete these " + selectedItems.Count + " item?");
            }
            else
            {
                await ShowModalAsync("Are you sure you want to delete this item?");
            }
            if (!_modalResult) return;

            foreach (var item in selectedItems)
            {
                _deleteOIDs.Add(item.ObjectId);
                _featureData.Remove(item);

            }
            NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
        }


        private async Task AddCommandExecuteAsync()
        {
            var newCN = new CancelledNumsData()
            {
                MapNumber = MapNumber,
                Taxlot = NewCancelledNum
            };

            var selectedItems = SelectedItemsList.Cast<CancelledNumsData>().ToList();
            if (selectedItems.Count() > 1)
            {
                await ShowModalAsync("You have multiple items selected. This item will be inserted before the first taxlot selected (" + selectedItems[0].Taxlot + "). Do you want to continue?");
                if (!_modalResult) return;
            }

            var index = (selectedItems.Count() == 0) ? 0 : FeatureData.IndexOf(selectedItems.First());
            _featureData.Insert(index, newCN);
            NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
        }


        private bool CanAddCommandExecute()
        {
            return FeatureData != null && !string.IsNullOrEmpty(NewCancelledNum) && FeatureData.Count(item => item.Taxlot == NewCancelledNum) == 0;
        }

        private void CloseModalCommandExecute(object obj)
        {
            var result = obj.ToString();
            _modalResult = result == "true" ? true : false;
            IsModalOpen = false;
        }


        private bool CanCloseModalCommandExecute()
        {
            return IsModalOpen;
        }


        private bool CanMoveCommandExecute(object obj)
        {
            if (FeatureData is null || !HasSelection()) return false;
            var dir = obj.ToString();
            var selectedItems = SelectedItemsList.Cast<CancelledNumsData>().ToList();
            var sortedItems = selectedItems.OrderBy(o => FeatureData.IndexOf(o)).ToList();
            if (dir.Contains("-") && FeatureData.IndexOf(sortedItems.Last()) == FeatureData.Count() - 1) return false;
            if (dir.Contains("+") && FeatureData.IndexOf(sortedItems.First()) == 0) return false;
            return true;
        }

        #endregion RelayCommandMethods


        #region PrivateMethods



        private async Task UpdateCancelledNumbersAsync()
        {
            
            await QueuedTask.Run(() =>
            {
                try
                {
                    var table = _activeMapView.Map.FindStandaloneTables("CancelledNumbers").First() as StandaloneTable;

                    var editOperation = new EditOperation();
                    editOperation.Name = "Delete Features";
                    editOperation.Delete(table, _deleteOIDs);
                    editOperation.Execute();

                    editOperation = new EditOperation();
                    editOperation.Name = "Add and Update Features";
                    foreach (var item in FeatureData.Select((value, i) => new { i, value }))
                    {
                        var attributes = new Dictionary<string, object>();
                        attributes.Add("MapNumber", MapNumber);
                        attributes.Add("Taxlot", item.value.Taxlot);
                        attributes.Add("SortOrder", item.i);

                        if (item.value.ObjectId == 0) //Item does not exist so add it
                        {
                            editOperation.Create(table, attributes);
                        }
                        else //Item exists so update it.
                        {
                            editOperation.Modify(table, item.value.ObjectId, attributes);
                        }
                    }
                    editOperation.Execute();
                    _featuresChanged = false;
                }
                catch (Exception ex)
                {
                    ShowErrors($@"Query error: {ex}");
                }
            });
        }



        private void GetMapNumbers()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    //-- Populate the MapNumbers AutoComplete Text Box.
                    var table = _activeMapView.Map.FindStandaloneTables("CancelledNumbers").First() as StandaloneTable;
                    var queryFilter = new QueryFilter();
                    queryFilter.WhereClause = "MAPNUMBER IS NOT NULL";
                    queryFilter.PrefixClause = "DISTINCT";
                    queryFilter.SubFields = "MAPNUMBER";

                    var list = new List<string>();
                    var results = table.Search(queryFilter);
                    using (RowCursor rowCursor = table.Search(queryFilter))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row current = rowCursor.Current)
                            {
                                list.Add(current["MAPNUMBER"].ToString());
                            }
                        }
                    }
                    MapNumbers = list;
                }
                catch (Exception ex)
                {
                    ShowErrors($@"Query error: {ex}");
                }
            });
        }


        private async Task<bool> TestPrereqsAsync(bool showErrors)
        {

            return await QueuedTask.Run(() =>
            {
                if (_activeMapView is null)
                    {
                    if (showErrors) ShowErrors("An active map must exist in this project to use this tool.");
                    return false;
                }

                if (_activeMapView.Map.FindStandaloneTables("CancelledNumbers").Count < 1)
                {
                    if (showErrors) ShowErrors("A table named 'CancelledNumbers' must exist in this project to use this tool.");
                    return false;
                }

                var table = _activeMapView.Map.FindStandaloneTables("CancelledNumbers").First() as StandaloneTable;
                var fields = table.GetFieldDescriptions();
                var reqFields = new List<string>() { "MapNumber", "Taxlot", "SortOrder" };
                foreach (var f in reqFields)
                {
                    if (fields.Find(x => x.Name.ToUpper() == f.ToUpper()) == null)
                    {
                        if (showErrors) ShowErrors("A field named '" + f + "' must exist in the CancelledNumbers table to use this tool. It is not case sesitive.");
                        return false;
                    }
                }
                return true;

            });
        }


        private bool HasSelection()
        {
            return SelectedItemsList.Cast<CancelledNumsData>().Count() > 0;
        }

        private void ClearDockPane(bool clearSearch)
        {
            _deleteOIDs = new List<long>();
            MapNumber = null;
            FeatureData = null;
            NewCancelledNum = null;
            NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
            if (clearSearch) SearchText = null;
        }

        private void ShowErrors(string ErrorMessage)
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ErrorMessage);
        }

        private async Task ShowModalAsync(string Message)
        {
            ModalMessage = Message;
            IsModalOpen = true;
            while (IsModalOpen)
            {
                await Task.Delay(25);
            }
        }

        #endregion PrivateMethods


        #region InternalMethods

        public static void Show()
        {
            _showClicked = true;
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Activate();
        }

        #endregion InternalMethods


    }


    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CancelledNumbersDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CancelledNumbersDockpaneViewModel.Show();
        }
    }
}
