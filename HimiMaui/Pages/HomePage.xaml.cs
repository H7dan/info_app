using Himi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Himi.Pages;

public partial class HomePage : ContentPage
{
	private readonly HomeViewModel _vm;
	private bool _suppressStubToggle;

	public HomePage()
	{
		InitializeComponent();
		_vm = App.Services.GetRequiredService<HomeViewModel>();
		BindingContext = _vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync();

		var settings = App.Services.GetRequiredService<Services.IAiSettingsService>();
		if (FindByName("StubSwitch") is Switch stubSwitch)
		{
			_suppressStubToggle = true;
			stubSwitch.IsToggled = settings.StubMode;
			_suppressStubToggle = false;
		}
	}

	private async void OnPhoneMenuClicked(object? sender, EventArgs e)
	{
		if (_vm.EmergencyContacts.Count == 0)
		{
			await DisplayAlertAsync(_vm.PhonesTitle, _vm.PhonesEmptyMessage, _vm.Ok);
			return;
		}

		var actionLabels = _vm.EmergencyContacts
			.Select(c => $"{c.Number} — {c.Title}")
			.ToArray();

		var picked = await DisplayActionSheetAsync(_vm.PhonesTitle, _vm.Cancel, null, actionLabels);
		if (string.IsNullOrWhiteSpace(picked) || picked == _vm.Cancel)
			return;

		var number = picked.Split('—')[0].Trim();

		try
		{
			PhoneDialer.Open(number);
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync(_vm.Error, ex.Message, _vm.Ok);
		}
	}

	private async void OnDialClicked(object? sender, EventArgs e)
	{
		if (sender is not Button { BindingContext: Models.PhoneContact contact })
			return;

		try
		{
			var confirm = await DisplayAlertAsync("Call", $"{contact.Title}\n{contact.Number}", "Call", "Cancel");
			if (!confirm) return;

			PhoneDialer.Open(contact.Number);
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Error", ex.Message, "OK");
		}
	}

	private async void OnMedicineClicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("Medicine", "MVP: add SOR/NPL + pharmacy info here.", "OK");
	}

	private async void OnFirstStepsClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("FirstSteps");
	}

	private async void OnHandbookClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("Handbook");
	}

	private async void OnNewsClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("News");
	}

	private async void OnChatClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("Chat");
	}

	private void OnStubToggled(object? sender, ToggledEventArgs e)
	{
		if (_suppressStubToggle)
			return;

		var settings = App.Services.GetRequiredService<Services.IAiSettingsService>();
		settings.SetStubMode(e.Value);
	}
}

