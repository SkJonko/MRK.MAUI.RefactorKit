
using CommunityToolkit.Mvvm.ComponentModel;

namespace XamlSamples;

public partial class TestViewModel : ObservableObject
{
	[ObservableProperty]
	public partial string Name { get; set; }
}