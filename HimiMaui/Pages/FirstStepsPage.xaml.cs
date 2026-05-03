using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

public partial class FirstStepsPage : ContentPage
{
	private readonly ViewModels.FirstStepsViewModel _vm;

	public FirstStepsPage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<ViewModels.FirstStepsViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync();
	}

	private async void OnChecklistSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not ViewModels.ChecklistListItemVm item)
			return;

		((CollectionView)sender!).SelectedItem = null;
		await Shell.Current.GoToAsync($"Checklist?checklistId={Uri.EscapeDataString(item.Id)}");
	}
}

