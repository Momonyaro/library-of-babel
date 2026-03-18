using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Babel.Enums;
using Babel.Services;
using Babel.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AudiobookPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand OpenAudiobookCommand { get; }
    public ICommand PlaySelectedAudiobookCommand { get; }
    public ICommand PlaySelectedAudiobookFromChapterCommand { get; }
    public ICommand OpenPlayerViewCommand { get; }
    public RelayCommand ClearSearchCommand { get; }
    public ICommand ResetToBrowseCommand { get; }
    public ICommand SetBrowseTabCommand { get; }
    public ICommand SetDetailsTabCommand { get; }
    public ICommand TogglePlaybackCommand { get; }
    public ICommand SkipForwardCommand { get; }
    public ICommand SkipBackwardCommand { get; }
    public ICommand SkipChapterForwardCommand { get; }
    public ICommand SkipChapterBackwardCommand { get; }
    public ICommand PlayFromChapterCommand { get; }
    public ICommand ToggleSelectedFavoriteCommand { get; }
    public ICommand ToggleSelectedFinishedCommand { get; }

    
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private MainSortingTab browseTab = MainSortingTab.Title;
    [ObservableProperty] private DetailsContentTab detailsContentTab = DetailsContentTab.Details;
    [ObservableProperty] private MainViews currentView = MainViews.Browse;
    [ObservableProperty] private AudiobookListItem[] books = [];
    [ObservableProperty] private AudiobookListItem[] filteredBooks = [];
    [ObservableProperty] private AudiobookDetailsTuple? selectedAudiobook = null;
    [ObservableProperty] private AudiobookPlayerData currentAudiobook = new(null);
    [ObservableProperty] private int volume = 100;
    [ObservableProperty] private bool selectedFavorited = false;
    [ObservableProperty] private bool selectedFinished = false;

    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged();
                ClearSearchCommand.RaiseCanExecuteChanged();
                ApplyFilter();
            }
        }
    }

    private MainViews _lastView = MainViews.Browse;
    private TimeSpan? _lastSaved;


    public MainWindowViewModel()
    {
        OpenAudiobookCommand = new RelayCommand<AudiobookListItem>(OnClick_AudiobookListItem);
        PlaySelectedAudiobookCommand = new RelayCommand(OnClick_PlaySelectedAudiobook);
        PlaySelectedAudiobookFromChapterCommand = new RelayCommand<int>(OnClick_PlaySelectedAudiobookFromChapter);
        OpenPlayerViewCommand = new RelayCommand(OnClick_OpenPlayer);
        ClearSearchCommand = new RelayCommand( () => SearchQuery = string.Empty, () => !string.IsNullOrWhiteSpace(SearchQuery) );
        SetBrowseTabCommand = new RelayCommand<string>(OnClick_SetBrowseTab);
        SetDetailsTabCommand = new RelayCommand<string>(OnClick_SetDetailsTab);
        ResetToBrowseCommand = new RelayCommand<object?>(OnClick_ResetView);
        TogglePlaybackCommand = new RelayCommand(OnClick_TogglePlayback);
        PlayFromChapterCommand = new RelayCommand<int>(OnClick_PlayFromChapter);
        SkipForwardCommand = new RelayCommand(OnClick_SkipForwardsSeconds);
        SkipBackwardCommand = new RelayCommand(OnClick_SkipBackwardsSeconds);
        SkipChapterForwardCommand = new RelayCommand(OnClick_SkipForwardsChapter);
        SkipChapterBackwardCommand = new RelayCommand(OnClick_SkipBackwardsChapter);
        ToggleSelectedFavoriteCommand = new RelayCommand(OnClick_ToggleFavorite);
        ToggleSelectedFinishedCommand = new RelayCommand(OnClick_ToggleFinished);

        RefreshLibrary();

        var latestListen = DataStoreService.Instance.GetLatestListen();
        if (latestListen != null)
        {
            var latestListenListItem = DataStoreService.Instance.GetAudiobookListItem(latestListen);
            CurrentAudiobook = new(latestListenListItem);
            MediaPlayerService.Instance.SetAudiobookNoPlay(latestListen);
        }

        IsLoading = LibraryScanService.Instance.Scanning;
        LibraryScanService.Instance.OnScanInitiated += OnLibraryScanInitiated;
        LibraryScanService.Instance.OnScanCompleted += OnLibraryScanCompleted;
        ImageScanService.Instance.OnScanCompleted += OnImagesFetched;
        MediaPlayerService.Instance.OnPlaybackChanged += OnPlaybackChanged;
        MediaPlayerService.Instance.OnMediaMounted += OnMediaMounted;
        MediaPlayerService.Instance.OnTimeUpdated += OnTimeChanged;
        MediaPlayerService.Instance.OnMediaEnd += OnMediaEndReached;

        _searchQuery = "";
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(BrowseTab))
        {
            RefreshLibrary();
        }
        if (e.PropertyName == nameof(Volume))
        {
            MediaPlayerService.Instance.SetVolume(Volume);
        }
    }

#region Events

    private void OnLibraryScanInitiated()
    {
        IsLoading = true;
    }

    private void OnLibraryScanCompleted()
    {
        RefreshLibrary();
        IsLoading = false;
    }

    private void OnImagesFetched()
    {
        RefreshLibrary();
    }

    private void OnMediaMounted()
    {
        var currentTime = MediaPlayerService.Instance.Time;
        _lastSaved = currentTime;

        OnPlaybackChanged();
        Volume = MediaPlayerService.Instance.Volume;
    }

    private void OnTimeChanged()
    {
        var currentTime = MediaPlayerService.Instance.Time;
        if (_lastSaved == null)
        {
            _lastSaved = currentTime;
        }

        var difference = (currentTime - _lastSaved.Value).Duration();
        
        // Console.WriteLine("tick: " + _lastSaved.ToString() + " | " + currentTime.ToString());

        if (difference > TimeSpan.FromSeconds(7))
        {
            _lastSaved = currentTime;
            DataStoreService.Instance.UpdateBookmark(CurrentAudiobook.CurrentTitle, currentTime);
            Console.WriteLine("Updated bookmark: " + CurrentAudiobook.CurrentTitle + " | " + currentTime.ToString());
        }
    }

    private void OnPlaybackChanged()
    {
        FilteredBooks = UpdatePlaybackStatus(FilteredBooks);
    }

    private void OnMediaEndReached()
    {
        RefreshLibrary();
    }

#endregion

#region Input Controls

    private void OnClick_AudiobookListItem(AudiobookListItem? listItem)
    {
        if (listItem == null)
            return;

        var match = DataStoreService.Instance.GetAudibookByName(listItem!.Title);
        if (match == null)
            return;

        SelectedAudiobook = new()
        {
            ListItem = listItem,
            Audiobook = match
        };

        _lastView = CurrentView;
        CurrentView = MainViews.Details;

        SelectedFavorited = match.Favorite!.Value;
        SelectedFinished  = match.Finished!.Value;
    }

    private void OnClick_PlaySelectedAudiobook()
    {
        CurrentAudiobook = new(SelectedAudiobook!.ListItem);

        MediaPlayerService.Instance.PlayAudiobook(SelectedAudiobook!.Audiobook);

        _lastView = CurrentView;
        CurrentView = MainViews.Player;
    }

    private void OnClick_PlaySelectedAudiobookFromChapter(int chapterID)
    {
        CurrentAudiobook = new(SelectedAudiobook!.ListItem);

        MediaPlayerService.Instance.PlayAudiobook(SelectedAudiobook!.Audiobook, chapterID);

        _lastView = CurrentView;
        CurrentView = MainViews.Player;
    }

    private void OnClick_OpenPlayer()
    {
        _lastView = CurrentView;
        CurrentView = MainViews.Player;
    }

    private void OnClick_TogglePlayback()
    {
        var isPlaying = MediaPlayerService.Instance.IsPlaying;
        if (isPlaying)
        {
            MediaPlayerService.Instance.Pause();
        }
        else
        {
            if (MediaPlayerService.Instance.HasMedia)
                MediaPlayerService.Instance.Resume();
        }
    }

    private void OnClick_SkipForwardsSeconds()
    {
        MediaPlayerService.Instance.SeekRelativeSeconds( 15.0f);
    }

    private void OnClick_SkipBackwardsSeconds()
    {
        MediaPlayerService.Instance.SeekRelativeSeconds(-15.0f);
    }

    private void OnClick_SkipForwardsChapter()
    {
        MediaPlayerService.Instance.SeekNextChapter();
    }

    private void OnClick_SkipBackwardsChapter()
    {
        MediaPlayerService.Instance.SeekLastChapter();
    }

    private void OnClick_PlayFromChapter(int chapterID)
    {
        MediaPlayerService.Instance.SeekToChapter(chapterID);
    }

    private void OnClick_SetBrowseTab(string? tab)
    {
        if (Enum.TryParse<MainSortingTab>(tab, out var result))
            BrowseTab = result;
            
    }

    private void OnClick_SetDetailsTab(string? tab)
    {
        if (Enum.TryParse<DetailsContentTab>(tab, out var result))
            DetailsContentTab = result;
    }

    private void OnClick_ResetView(object? _)
    {
        CurrentView = _lastView;
        _lastView = MainViews.Browse;
        DetailsContentTab = DetailsContentTab.Details;

        if (CurrentView == MainViews.Browse)
        {
            OnPlaybackChanged();
        }
    }

    private void OnClick_ToggleFavorite()
    {
        if (SelectedAudiobook == null)
            return;

        var selected = SelectedAudiobook!;
        selected!.Audiobook!.Favorite = !selected!.Audiobook!.Favorite;
        DataStoreService.Instance.SetAudiobookFavorite(selected.ListItem!.Title, selected!.Audiobook!.Favorite!.Value);

        SelectedFavorited = selected!.Audiobook!.Favorite.Value;
        RefreshLibrary();
    }

    private void OnClick_ToggleFinished()
    {
        if (SelectedAudiobook == null)
            return;

        var selected = SelectedAudiobook!;
        selected!.Audiobook!.Finished = !selected!.Audiobook!.Finished;
        DataStoreService.Instance.SetAudiobookFinished(selected.ListItem!.Title, selected!.Audiobook!.Finished!.Value);

        SelectedFinished = selected!.Audiobook!.Finished.Value;
        RefreshLibrary();
    }

#endregion

    private void ApplyFilter()
    {
        List<AudiobookListItem> temp = [];

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            foreach (var book in Books)
                temp.Add(book);
            
            FilteredBooks = [.. temp];
            return;
        }

        var query = SearchQuery.Trim().ToLower();

        foreach (var book in Books)
        {
            if ((book.Title?.ToLower().Contains(query) ?? false) ||
                (book.Author?.ToLower().Contains(query) ?? false) ||
                (book.Series?.ToLower().Contains(query) ?? false))
            {
                temp.Add(book);
            }
        }

        FilteredBooks = [..temp];
    }

    private AudiobookListItem[] UpdatePlaybackStatus(AudiobookListItem[] audiobooks)
    {
        var isPlaying = MediaPlayerService.Instance.IsPlaying;
        var hasMedia = MediaPlayerService.Instance.HasMedia;
        var media = MediaPlayerService.Instance.CurrentlyPlaying;

        AudiobookListItem[] tempBooks = [..audiobooks];
        foreach (var book in tempBooks)
        {
            book.IsPlaying = hasMedia && book.Title == media!.Title;
        }

        return tempBooks;
    }

    private Comparison<AudiobookListItem> SortByCurrentTab()
    {
        return BrowseTab switch
        {
            MainSortingTab.Title => (a, b) => AudiobookComparer.Instance.CompareTitles(a, b),
            MainSortingTab.Author => (a, b) => string.Compare(a.Author, b.Author, StringComparison.OrdinalIgnoreCase),
            MainSortingTab.Series => (a, b) => AudiobookComparer.Instance.CompareSeries(a, b),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void RefreshLibrary()
    {
        var tempBooks = DataStoreService.Instance.GetStoredBooksAsListItems().ToList();
        tempBooks.Sort(SortByCurrentTab());
        var tempBooksArr = UpdatePlaybackStatus([..tempBooks]);

        Books = tempBooksArr;
        FilteredBooks = tempBooksArr;
        ApplyFilter();
    }
}
