<%@ Control Language="C#" AutoEventWireup="true" Inherits="Jhu.Graywulf.SciDrive.ImportTablesFromSciDriveForm" Codebehind="ImportTablesFromSciDriveForm.ascx.cs" %>

<p>
    Specify a file on SciDrive
</p>
<table class="FormTable">
    <tr>
        <td class="FormLabel" style="width: 50px">
            <asp:Label runat="server" ID="uriLabel">Path:</asp:Label>&nbsp;&nbsp;
        </td>
        <td class="FormField" style="width: 420px">
            <asp:TextBox runat="server" ID="uri" CssClass="FormField" Width="420px" Text="first_container/myfile.csv" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="uri" Display="Dynamic"
                ErrorMessage="<br />The URI field is required" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="uri" Display="Dynamic"
                ErrorMessage="<br />Invalid SciDrive path" ValidationExpression="^/?[a-zA-Z0-9_\.]+(/[a-zA-Z0-9_\.]+)*$" />
        </td>
    </tr>
</table>
