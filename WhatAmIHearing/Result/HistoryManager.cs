using System.Collections.ObjectModel;
using ZemotoCommon;

namespace WhatAmIHearing.Result;

internal sealed class HistoryManager
{
   private static readonly SystemFile _historyFile = new( "history.json" );

   public void Load() => History = _historyFile.DeserializeContents<ObservableCollection<SongViewModel>>() ?? [];

   public void Save() => _historyFile.SerializeInto( History );

   public void Delete( SongViewModel song ) => History.Remove( song );

   public ObservableCollection<SongViewModel> History { get; private set; }
}
