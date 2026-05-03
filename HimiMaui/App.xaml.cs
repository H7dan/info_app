namespace Himi;

public partial class App : Application
{
	public static IServiceProvider Services { get; private set; } = default!;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		Services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(Services.GetRequiredService<AppShell>());
	}
}