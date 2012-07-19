using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Utils;
using System.Linq.Expressions;

namespace HexViewer
{
	public class HexViewerViewModel : PropertyChangedViewModel
	{
		public HexViewerViewModel()
		{
			Task.Factory.StartNew(() =>
				{
					return Directory.EnumerateFiles(@"c:\windows\winsxs", "*", SearchOption.AllDirectories).ToList();
				})
				.ContinueWith(t =>
					{
						this.Files = t.Result.Select(s => new FileViewModel() { Path = s }).ToArray();
						OnPropertyChanged(() => this.Files);
					}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public FileViewModel[] Files { get; private set; }

		public class FileViewModel : PropertyChangedViewModel
		{
			public string Path { get; set; }

			private Task _LoadDataTask = null;
			private WeakReference _HexLinesRef = new WeakReference(null);
			public string[] HexLines
			{
				get
				{
					if (_LoadDataTask != null) return null;

					if (_HexLinesRef.Target == null)
					{
						_LoadDataTask = Task.Factory.StartNew(() => GetHexLines()).ContinueWith(t =>
							{
								_LoadDataTask = null;
								_HexLinesRef = new WeakReference(t.Result);
								OnPropertyChanged(() => HexLines);
							}, TaskScheduler.FromCurrentSynchronizationContext());
					}

					return _HexLinesRef.Target as string[];
				}
			}

			private string[] GetHexLines()
			{
				var lines = new List<string>();
				var bytes = File.ReadAllBytes(this.Path);
				var len = bytes.Length;
				int i = 0;
				int perLine = 16;
				while (true)
				{
					var builder = new StringBuilder();
					perLine = Math.Min(perLine, len - i);
					for (int j = 0; j < perLine; j++)
					{
						builder.AppendFormat("{0:x2}", bytes[i++]);
					}
					lines.Add(builder.ToString());

					if (i >= len) break;
				}
				return lines.ToArray();
			}
		}
	}
}
