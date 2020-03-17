var currentQuestion = -1;
var numberOfQuestions = 0;
var questionDatas = [];
var preloadAnswers = [];
var statusClass = {
    "current": "btn btn-warning btn-sm",
    "valid": "btn btn-success btn-sm",
    "invalid": "btn btn-danger btn-sm"
};

var statusBar = [];


function updateStatus(questionId, status) {
    var statusNode = document.getElementById("status-" + questionId);
    statusNode.setAttribute("class", statusClass[status]);
}

function addToStatusBar(questionId) {
    statusBar.push("valid");
    var statusContainerNode = document.getElementById("question-status");
    var statusNode = document.createElement("button");
    statusNode.setAttribute("type", "button");
    statusNode.setAttribute("class", statusClass["valid"]);
    statusNode.setAttribute("id", "status-" + questionId);
    statusNode.style.visibility = "hidden";

    statusNode.addEventListener("click", function () {
        changeQuestionTo(this);
    });
    statusNode.innerHTML = (questionId === numberOfQuestions) ? "Result" : ((questionId + 1).toString());
    statusContainerNode.appendChild(statusNode);
    statusContainerNode.appendChild(document.createTextNode("\u00A0"));
}

function createSymbolNode(type) {
    var symbolNode = document.createElement("span");
    symbolNode.setAttribute("class", "glyphicon glyphicon-" + type);
    symbolNode.setAttribute("aria-hidden", "true");
    symbolNode.innerHTML = " ";
    return symbolNode;
}

function createQuestionTextNode(questionText) {

    // create tr node
    var rowNode = document.createElement("tr");

    // create td node
    var tdNode = document.createElement("td");
    tdNode.setAttribute("colspan", "2");

    // create blockquote node
    var blockQuoteNode = document.createElement("blockquote");

    // create question text node
    var questionTextNode = document.createElement("p");
    questionTextNode.setAttribute("class", "lead");
    questionTextNode.innerHTML = questionText;

    // build block
    blockQuoteNode.appendChild(questionTextNode);
    tdNode.appendChild(blockQuoteNode);
    rowNode.appendChild(tdNode);

    return rowNode;
}


function createOptionNode(questionId, optionId, optionText) {
    // create tr node
    var rowNode = document.createElement("tr");

    // create td node
    var tdNode = document.createElement("td");
    tdNode.setAttribute("colspan", "2");

    // create option block node
    var optionBlockNode = document.createElement("p");
    optionBlockNode.setAttribute("class", "text-lg");

    // create radio node
    var radioNode = document.createElement("input");
    radioNode.setAttribute("type", "radio");
    radioNode.setAttribute("name", "question-" + questionId + "-option");
    radioNode.setAttribute("value", "option-" + optionId);

    // attach all
    optionBlockNode.appendChild(radioNode);
    optionBlockNode.appendChild(document.createTextNode(" " + optionText));

    tdNode.appendChild(optionBlockNode);
    rowNode.appendChild(tdNode);

    return rowNode;
}
function createCheckNode(questionId, optionId, optionText) {
    // create tr node
    var rowNode = document.createElement("tr");

    // create td node
    var tdNode = document.createElement("td");
    tdNode.setAttribute("colspan", "2");

    // create option block node
    var optionBlockNode = document.createElement("p");
    optionBlockNode.setAttribute("class", "text-lg");

    // create radio node
    var radioNode = document.createElement("input");
    radioNode.setAttribute("type", "checkbox");
    radioNode.setAttribute("name", "question-" + questionId + "-option");
    radioNode.setAttribute("value", "option-" + optionId);

    // attach all
    optionBlockNode.appendChild(radioNode);
    optionBlockNode.appendChild(document.createTextNode(" " + optionText));

    tdNode.appendChild(optionBlockNode);
    rowNode.appendChild(tdNode);

    return rowNode;
}


function renderQuestion(questionId, questionData) {
    console.log(questionData);
    var text = questionData["pitanje"];


    var options = questionData["odgovori"];
    var type = questionData["vrsta"];
    var controlNode = document.getElementById("quiz-control");
    var tableNode = controlNode.parentNode;

    // Create tbody
    var sectionNode = document.createElement("tbody");
    sectionNode.setAttribute("id", "question-" + questionId + "-section");
    sectionNode.setAttribute("hidden", "");
    sectionNode.appendChild(createQuestionTextNode(text));

    if (type === "one") {
        for (var optionId = 0; optionId < options.length; optionId++) {
            var optionText = options[optionId];
            sectionNode.appendChild(createOptionNode(questionId, optionId, optionText));
        }
    }
    if (type === "mul") {
        for (var _optionId = 0; _optionId < options.length; _optionId++) {
            var _optionText = options[_optionId];
            sectionNode.appendChild(createCheckNode(questionId, _optionId, _optionText));
        }
    }
    
    // attach before control node
    tableNode.insertBefore(sectionNode, controlNode);
}

function extractQuestionsData(questionsData) {
    for (var index = 0; index < 1; index++) {
        
        renderQuestion(index, questionsData);
    }
    setPreloadedAnswers();
}

function setPreloadedAnswers() {
    preloadAnswers = JSON.parse(document.getElementById("preload").value);
    console.log(preloadAnswers);
    var index = 0;
    for (var questionId = 0; questionId < 1; questionId++) {

        if (preloadAnswers === -1) continue;
        


        var optionNodes;
        if (questionDatas["vrsta"] === "mul") {
            optionNodes = getOptionCheckboxNodes(questionId);
        }
        else {
            optionNodes = getOptionRadioNodes(questionId);
        }
        console.log(optionNodes.length);
        // iterate over all radio button
        for (i = 0; i < preloadAnswers.length; i++) {
            
            optionNodes[preloadAnswers[i]].checked = true;

            
        }
    }
}


function setCurrentQuestionTo(questionId) {
    if (currentQuestion !== -1) {
        // Update status bar
        updateStatus(currentQuestion, checkValidity(currentQuestion, false));
        // Hide current question
        var currentQuestionNode = document.getElementById("question-" + currentQuestion + "-section") || document.getElementById("result-section");
        currentQuestionNode.setAttribute("hidden", "");
    }

    // Change current question
    currentQuestion = questionId;
    // Update title with current question
    document.getElementById("current-question-title").innerHTML = (currentQuestion === numberOfQuestions) ? "Result" : "Pitanje " + (currentQuestion + 1);
    document.getElementById("current-question-title").hidden = true;
    // Remove hidden attribute from current question
    var newCurrentQuestionNode = document.getElementById("question-" + currentQuestion + "-section") || document.getElementById("result-section");
    newCurrentQuestionNode.removeAttribute("hidden");

    updateStatus(questionId, "current");

   
}

function renderStatus() {
    for (var questionId = 0; questionId < numberOfQuestions; questionId++) {
        addToStatusBar(questionId);
    }
}

function changeQuestionTo(target) {
    var questionId = (target.innerHTML !== "Result") ? (parseInt(target.innerHTML) - 1) : numberOfQuestions;
    setCurrentQuestionTo(questionId);
}

function nextQuestion() {
    setCurrentQuestionTo(currentQuestion + 1);
}

function getQuestionsDataFromStorage() {
    //var questionsData = JSON.parse(localStorage.getItem("survey"));
    //storeToStorage(document.getElementById("survey").val);
    //console.log(document.getElementById("survey").value);
    var id = document.getElementById("idanketa").value;



    var data = JSON.parse(document.getElementById("survey").value);
    console.log(data.toString());
    var questionsData = data;
    //console.log(questionsData);
    questionDatas = questionsData;
    numberOfQuestions = 1;

    // extract and render all questions and options
    extractQuestionsData(questionsData);


    // Render status questions
    renderStatus(numberOfQuestions);
    updateAllStatus(false);
    setCurrentQuestionTo(0);





}


function getOptionRadioNodes(questionId) {
    return document.querySelectorAll("#question-" + questionId + "-section > tr > td > p > input[type=\"radio\"]");
}
function getOptionCheckboxNodes(questionId) {
    return document.querySelectorAll("#question-" + questionId + "-section > tr > td > p > input[type=\"checkbox\"]");
}


function checkValidity(questionId, finish) {


    // assert question text not blank
    var question = questionDatas[questionId];
    if (questionDatas["vrsta"] === "mul")
        return true;
    optionNodes = getOptionRadioNodes(questionId);
    
    // iterate over all radio button
    var countChecked = 0;
    for (index = 0; index < optionNodes.length; index++) {
        var optionNode = optionNodes[index];
        if (optionNode.checked) {
            countChecked++;
        }
    }

    return (countChecked > 0) ? "valid" : "invalid";


}


function updateAllStatus(finish) {
    for (var questionId = 0; questionId < numberOfQuestions; questionId++) {
        updateStatus(questionId, checkValidity(questionId, finish));
    }
}

function finish() {
    var controlNode = document.getElementById("quiz-control");
    var tableNode = controlNode.parentNode;

    var answers = [];

    for (questionId = 0; questionId < numberOfQuestions; questionId++) {
        // Check if valid
        if (checkValidity(questionId, true) === "invalid") {
            alert("Obavezna pitanja moraju se odgovoriti da bi se odgovori mogli spremiti.");
            return false;
        }

        var question = questionDatas[questionId];
        var answer = "";
        var optionNodes;
        
        if (questionDatas["vrsta"] === "mul") {
            optionNodes = getOptionCheckboxNodes(questionId);

        }
        if (questionDatas["vrsta"] === "one") {
            optionNodes = getOptionRadioNodess(questionId);

        }
        // iterate over all radio button
        
        for (index = 0; index < optionNodes.length; index++) {
            var optionNode = optionNodes[index];
            if (optionNode.checked) {
                answer += ""+ index + " ";
            }
        }
        
        //console.log("Question " + questionId + ": " + answer);
        answers = answer;
    }

    console.log(answers);
    //console.log('{"answers":' + JSON.stringify(answers) + '}');
    document.getElementById("answers").innerHTML = answers + '';
    //console.log(document.getElementById("answers"));
    document.getElementById("forma").submit();
    

}

function addSurveyControlEventListener() {
    // Add Option Button
    
    // Add Finish Button
    document.querySelector("#quiz-control > tr > td > button.btn.btn-success.btn-lg").addEventListener("click", function () {
        finish();
    });
}

(function () {
    getQuestionsDataFromStorage();
    addSurveyControlEventListener();

})();
