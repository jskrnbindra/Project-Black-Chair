/*
* This file has the code that executes and helps the SearchBox functionality 
*/
function getSearchBoxResults()
    {
        ///BEGIN/// Validation
        //not more than 20 words allowed
        var UserSearchQuery = $("#tb_SearchQuery").val();
        if (UserSearchQuery == "") {
            console.log("empty");
            return;
        }

        var WordCount = UserSearchQuery.trim().split(' ');
        var WordCounter=0;
        var flag_WordLimitExceeded = false;

        for(var c =0;c<WordCount.length;c++)
        {
            if(WordCounter>20) {flag_WordLimitExceeded=true;break;}
            if(WordCount[c]!="")
                WordCounter++;
        }

        if (flag_WordLimitExceeded) {
            console.log(WordCounter + " limit exceed words bro");
            return;
        }
        else
            console.log(WordCounter + " words. Good to go bro");
        ///END/// Validation


        $.ajax(
            {
                url: ResolveUrl('~/SearchBox.aspx/givePapersFromSearchQuery'),
                data: "{'UserSearchQuery':'" + $("#tb_SearchQuery").val() + "'}",
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    data = data.d;

                    document.getElementById("output").innerText = "";
                    console.log("SEARCH RESULTS");
                    showSearchResults(data[0],"Strict");//displaying search results
                    console.log("\n\n");
                    console.log("RECOMMENDED");
                    showSearchResults(data[1], "EasyRecs");//displaying recommendations
                },
                error: function (response) {
                    console.log(response);
                },
                failure: function (response) {
                    console.log("failure"+response);
                }
            });
    }

function showSearchResults(data,isRecommendation)
{
    var PaperOB = JSON.parse(data);//getting Strict Search Results

    var ResultType;

    console.log(isRecommendation);

    switch (isRecommendation)
    {
        case "Strict":
            ResultType = "Search Results: ";
            break;
        case "EasyRecs":
            ResultType = "Recommendations: ";
            break;
        default:

            ResultType = "Ambiguous Result Type Specifier: ";
            console.log("Error in Output type Specification");
    }
              
    document.getElementById("output").innerText += "\n\n"+ResultType+" "+PaperOB.Message + "\n" + PaperOB.NumberOfPapersFound + " Paper(s) Found. ";

    console.log(PaperOB["Papers"].length + " Course Code(s)");
    console.log("\n");
    //Accessing JSON response sequentially
    for (var c = 0; c < PaperOB["Papers"].length; c++) {
        var ccOBJ = PaperOB["Papers"][c];
        console.log(ccOBJ[Object.keys(ccOBJ)].length + " type(s) of paper(s) found in " + Object.keys(ccOBJ)[0]);//printing course code name

        for (var d = 0; d < ccOBJ[Object.keys(ccOBJ)].length; d++)//each papertype under a course code
        {
            var ptOBJ = ccOBJ[Object.keys(ccOBJ)][d];
            console.log(ptOBJ[Object.keys(ptOBJ)].length + " " + Object.keys(ptOBJ)[0] + " papers");//printing paper type 

            for (var e = 0; e < ptOBJ[Object.keys(ptOBJ)].length; e++)//each paper under a paper type 
            {
                console.log(ptOBJ[Object.keys(ptOBJ)][e]);
            }
        }
        console.log("\n");
    }
}

var isSuggestionDisplayed=false;//flag to see if there is any suggestion displayed to enable down arrow and shortcut selection of suggested course name
var lastSearchBoxText;

function isValid(UserInput)//this function will never recieve empty userInput because used with && in the suggestions if statement
{
    var InvalidChars = ['\'', '\\', '\"','<','>','{','}',';',':','/'];
            
    for (var c = 0; c < UserInput.length;c++)
        for (var d = 0; d < InvalidChars.length;d++) {
            if (UserInput.charAt(c) == InvalidChars[d])
                return false;
        }
    return true;
}

function autocompleteCourseName(e)
{
    var keyPressed = e.which || e.keyCode;//capturing the pressed key
    SearchBoxText = $("#tb_SearchByCourseName").val();
    lastSearchBoxText = SearchBoxText;//updating value in global variable
            

    if (isSuggestionDisplayed && e.ctrlKey && keyPressed == 32)//if ctrl+Enter pressed and there is a suggestion
    {
        $("#tb_SearchByCourseName").val($("#tb_SearchByCourseNameBacker").val());
        $("#AutocompleteMSG").hide();
        return;
    }


    if (((keyPressed > 47 && keyPressed < 58) || (keyPressed > 64 && keyPressed < 91) || (keyPressed == 189) || (keyPressed == 191) || (keyPressed == 32) || (keyPressed == 8 && SearchBoxText.length>0))&& isValid(SearchBoxText))//numbers or alphabets or '-' or '/' or ' ' or BACKSPACE if not all erased
    {

        $("#tb_SearchByCourseNameBacker").val('');//clearing backer text to avoid overlapping text  in case of slow connections

        var IncompleteCourseName = "{'prefix':'" + SearchBoxText + "'}";

        $.ajax(
            {
                url: ResolveUrl('~/SearchBox.aspx/getAutocompleteSuggestion'),
                method: 'POST',
                data: IncompleteCourseName,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    data = data.d;
                    console.log(data);
                    if (data == "none") {
                        isSuggestionDisplayed = false;
                        $("#tb_SearchByCourseNameBacker").val("");
                        $("#AutocompleteMSG").hide();
                    }
                    else {
                        isSuggestionDisplayed = true;
                        data = SearchBoxText + data.substring(SearchBoxText.length);
                        $("#tb_SearchByCourseNameBacker").val(data);
                            $("#AutocompleteMSG").show();
                    }
                },
                error: function (data) {
                    console.log("here in th error part")
                },
                failure: function (data) {
                    console.log("here in th failure part");
                }
            }
            );


//        if ($("#tb_SearchByCourseName").val().toLowerCase() == $("#tb_SearchByCourseNameBacker").val().toLowerCase())
  //          $("#AutocompleteMSG").hide();
    }
    else if (keyPressed == 16 || keyPressed == 17 || keyPressed == 20 || (keyPressed > 36 && keyPressed < 40))//if caps or shift or arrow keys
    { }
    else
    {
        $("#tb_SearchByCourseNameBacker").val("");
        $("#AutocompleteMSG").hide();
    }

    if (SearchBoxText == "")//setting the flag
    {
        isSuggestionDisplayed = false;
        $("#tb_SearchByCourseNameBacker").val("");
        $("#AutocompleteMSG").hide();
    }
}

function upperFocusOut()
{
    $("#tb_SearchByCourseNameBacker").val("");
    $("#AutocompleteMSG").hide();
    $("#tb_SearchByCourseNameBacker").remove();
}

function shiftFocus() {
    $("#tb_SearchByCourseNameBacker").focusout();
    $("#tb_SearchByCourseName").focusin();
}

function prepareForAutocomplete()
{
    $("#tb_SearchByCourseName").before('<input type="text" id="tb_SearchByCourseNameBacker" runat="server" class="backerTB" onfocus="shiftFocus()" />');
    //adding before to avoid 'Press tab to go to button' ambiguous behaviour
}

function getSearchByCourseNameResults()
{
    var SearchCourseName = $("#tb_SearchByCourseName").val();

    if (!isValid(SearchCourseName)) {
        $("#output").text("use of restricted characters detected");
        return;
    }
    if (SearchCourseName == "") {
        $("#output").text("empty query has no results");
        return;
    }

        var CourseName = "{'CourseName':'" + SearchCourseName + "'}";

        $.ajax(
            {
                url: ResolveUrl('~/SearchBox.aspx/getCourseNameSearchResults'),
                method: 'POST',
                data: CourseName,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    data = data.d;

                    document.getElementById("output").innerText = "";
                    console.log("SEARCH RESULTS by CourseName");
                    showSearchResults(data[0], "Strict");//displaying search results
                    console.log("\n\n");
                    console.log("RECOMMENDATIONS by CourseName");
                    showSearchResults(data[1], "EasyRecs");//displaying recommendations
                },
                error: function (data) {
                    console.log("here in th error part")
                },
                failure: function (data) {
                    console.log("here in th failure part");
                }
            }
            );
                
}
function ResolveUrl(url)
{
    if (url.indexOf("~/") == 0)
        url = rootURL + url.substring(2);

    return url;
}