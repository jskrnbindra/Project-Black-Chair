
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchBox.aspx.cs" Inherits="DataCollection.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>
        Search Box :: BlackChair
    </title>
      <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
    <link href="Styler.css" rel="stylesheet" />
    <link href="SearchBoxStyler.css" rel="stylesheet" />
    <script>
        var rootURL = "<%= ResolveUrl("~/") %>";
        //enables server side ResolveURL in client side 
    </script>
    <script src="SearchBoxHelper.js"></script>
</head>
<body>
    <form id="form1" runat="server">
           <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
          <br />
            <h2>Search Box (beta)</h2>
            <h1 style="margin-bottom: -10px; font-size: 2em;">Search Papers in BlackChair</h1>
            <br />
        <div id="SearchBoxHolder" style="background: #fff">
                     <input type="text" id="tb_SearchByCourseName" runat="server" class="backerTB overer" onfocus="prepareForAutocomplete()" onfocusout="upperFocusOut()" onkeyup="autocompleteCourseName(event)" />
           <br />
        <br />
<input type="button" id="btn_searchByCourseName" value="Search by Course Name" onclick="getSearchByCourseNameResults()" />
            <small style="background:yellow;font-size:.8em;display: none" id="AutocompleteMSG">Ctrl+space to autocomplete</small>
        </div>
        <br />
        <br />

        <asp:TextBox ID="tb_SearchQuery" type="Search" runat="server"  Width="500" PlaceHolder="Search Anything (MTE papers of CSE222)"></asp:TextBox>
        <!-- 
            Client side validation to filter inputs with single or double quotest
            pattern="[^'\x22]+" 
            -->
        <input type ="button" onclick="getSearchBoxResults()" value="Search by CourseCode" />

        <p id="output" style="background: #fff;"></p>
        <br />
        <br />
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>