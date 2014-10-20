using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Ektron.Cms;
using Ektron.Cms.Framework.Organization;
using Ektron.Cms.Widget;

/// <summary>
/// An Ektron workarea widget to display a customizable list of "favorite folders" for the current logged in user.
/// </summary>
public partial class widgets_FavoriteFolders : WorkareaWidgetBaseControl, IWidget
{
    /// <summary>
    /// Models a single chosen "favorite folder"
    /// </summary>
    private class FavoriteFolderRecord
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string PathLink { get; set; }
    }

    /// <summary>
    /// The set of favorite folders for the currently logged in user.
    /// </summary>
    [WidgetDataMember(new string[0])]
    public string[] Favorites { get; set; }

    protected override void OnInit(EventArgs e)
    {
        //boilerplate widget code
        base.OnInit(e);
        base.Host.Edit += new EditDelegate(EditEvent);
        base.Host.Maximize += new MaximizeDelegate(delegate() { Visible = true; });
        base.Host.Minimize += new MinimizeDelegate(delegate() { Visible = false; });
        base.Host.Create += new CreateDelegate(delegate() { EditEvent(""); });
        ViewSet.SetActiveView(uxWidgetView);
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        //if the user is viewing their selected list of favorite folders
        if (this.ViewSet.ActiveViewIndex == 0)
        {
            //load this user's data
            var folderData = this.loadFolderData();
            var dataSource = this.loadWidgetData(folderData);
            this.bindWidget(dataSource);
        }

        //sets the title bar of the widget
        string title = "<strong>Favorite Folders</strong> (click the check at the far right to add folders to this list)";
        base.SetTitle(title);
    }

    /// <summary>
    /// Event for saving a user's list of favorite folders.
    /// </summary>
    protected void SaveButton_Click(object sender, EventArgs e)
    {
        //get the (string) ids for each folder the user has checked
        this.Favorites = FolderTreeView.CheckedNodes
                                       .Cast<TreeNode>()
                                       .Select(node => node.Value)
                                       .ToArray();

        //persist this data to the database
        Host.SaveWidgetDataMembers();

        //reload the widget view with their now saved preferences
        ViewSet.SetActiveView(uxWidgetView);
    }

    /// <summary>
    /// Event for cancelling a selection of favorite folders.
    /// </summary>
    protected void CancelButton_Click(object sender, EventArgs e)
    {
        //just redisplay the widget view.
        ViewSet.SetActiveView(uxWidgetView);
    }

    /// <summary>
    /// Builds a UI for selecting favorite folders from a list of all possible folders.
    /// </summary>
    protected void EditEvent(string settings = "")
    {
        //the complete folder tree in Ektron
        var folderTree = new FolderManager().GetTree(0);
        
        //build a .NET TreeNode representation of the Ektron folder structure
        var rootNode = this.buildNodeHeirarchy(folderTree);

        //bind this TreeNode to an ASP.NET TreeView control
        FolderTreeView.Nodes.Clear();
        FolderTreeView.Nodes.Add(rootNode);

        //make sure the root is expanded
        rootNode.Expand();

        //now expand all selected child nodes
        var selected = FolderTreeView.CheckedNodes.Cast<TreeNode>();
        foreach (var node in selected)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                parent.Expand();
                parent = parent.Parent;
            }
        
        }

        //finally, display the edit view
        //(e.g. the ASP.NET TreeView control with checkboxes for each folder
        ViewSet.SetActiveView(uxEditView);
    }

    /// <summary>
    /// Get a collection of Ektron FolderData for the current user's selected favorites
    /// </summary>
    /// <returns></returns>
    private IEnumerable<FolderData> loadFolderData()
    {
        var favorites = new List<FolderData>();

        if (this.Favorites != null && this.Favorites.Length > 0)
        {
            var criteria = new FolderCriteria();
            criteria.AddFilter(Ektron.Cms.Common.FolderProperty.Id, Ektron.Cms.Common.CriteriaFilterOperator.In, this.Favorites);
            favorites = new FolderManager().GetList(criteria);
        }

        return favorites;
    }

    /// <summary>
    /// Convert a collection of Ektron FolderData into a collection of FavoriteFolderRecord
    /// </summary>
    private IEnumerable<FavoriteFolderRecord> loadWidgetData(IEnumerable<FolderData> folders)
    {
        var records = new List<FavoriteFolderRecord>();
        string pathPattern = @"<a href=""#"" onclick=""top.showContentInWorkarea('content.aspx?action=ViewContentByCategory&id={0}&LangType={1}', 'Content', '{2}')"">/{3}</a>";

        foreach (FolderData folder in folders)
        {
            string folderCSVPath = EkContentRef.GetFolderParentFolderIdRecursive(folder.Id);

            var record = new FavoriteFolderRecord() {
                ID = folder.Id,
                Name = folder.Name,
                Path = folder.NameWithPath,
                PathLink = String.Format(pathPattern, folder.Id, 1033, folderCSVPath, folder.NameWithPath)
            };

            records.Add(record);
        }

        return records.OrderBy(f => f.Path);
    }

    /// <summary>
    /// Binds a collection of FavoriteFolderRecord to the UI, allowing the user to navigate to the selected folders.
    /// </summary>
    private void bindWidget(IEnumerable<FavoriteFolderRecord> dataSource)
    {
        if (dataSource.Count() > 0)
        {
            DataGrid.DataSource = dataSource;
            DataGrid.DataBind();
            DataGrid.Visible = true;
            NoRecordsLabel.Visible = false;
        }
        else
        {
            NoRecordsLabel.Visible = true;
        }
    }

    /// <summary>
    /// Builds a .NET TreeNode representation of the Ektron FolderData, ensuring any "favorited" sub-folders show up as "checked".
    /// </summary>
    private TreeNode buildNodeHeirarchy(FolderData folderData)
    {
        string fID = folderData.Id.ToString();
        var node = new TreeNode(folderData.Name, fID);

        node.Checked = Favorites.Contains(fID);

        if (folderData.HasChildren)
        {
            foreach (var childFolder in folderData.ChildFolders)
            {
                var childNode = buildNodeHeirarchy(childFolder);
                node.ChildNodes.Add(childNode);
            }
        }

        return node;
    }
}
