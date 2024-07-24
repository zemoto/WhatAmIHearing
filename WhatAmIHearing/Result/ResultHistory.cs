using System.Collections.ObjectModel;
using ZemotoCommon;

namespace WhatAmIHearing.Result;

internal sealed class ResultHistory : ObservableCollection<SongViewModel>
{
   private static readonly SystemFile _historyFile = new( "history.json" );

   public static ResultHistory Load() => _historyFile.DeserializeContents<ResultHistory>() ?? [];

   public void Save() => _historyFile.SerializeInto( this );
}
