namespace Himi;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Route-based navigation is kept for later deep-links (articles/checklist items).
		Routing.RegisterRoute("FirstSteps", typeof(Pages.FirstStepsPage));
		Routing.RegisterRoute("Checklist", typeof(Pages.ChecklistPage));
		Routing.RegisterRoute("Handbook", typeof(Pages.HandbookPage));
		Routing.RegisterRoute("HandbookCategory", typeof(Pages.HandbookCategoryPage));
		Routing.RegisterRoute("Article", typeof(Pages.ArticlePage));
		Routing.RegisterRoute("Contacts", typeof(Pages.ContactsPage));
		Routing.RegisterRoute("News", typeof(Pages.NewsPage));
		Routing.RegisterRoute("Chat", typeof(Pages.ChatPage));
	}
}
