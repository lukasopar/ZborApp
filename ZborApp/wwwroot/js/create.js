/**
 * Created by zeulb on 8/10/15.
 */

var currentQuestion = -1;
var numberOfQuestion = 0;
var numberOfOptionList = [];
var statusClass = {
    "current": "btn btn-warning btn-sm",
    "valid": "btn btn-success btn-sm",
    "invalid": "btn btn-danger btn-sm"
};

function storeToStorage(obj) {
    localStorage.clear();
    for (var prop in obj) {
        if (obj.hasOwnProperty(prop)) {
            localStorage.setItem(prop, obj[prop]);
        }
    }
}

function createNewLineNode() {
    return document.createElement("br");
}

function createOptionNode(questionId, optionId) {

    // create <tr>
    var trNode = document.createElement("tr");

    // create <td colspan="2">
    var tdNode = document.createElement("td");
    tdNode.setAttribute("colspan", "2");

    
    // create <div class="radio">
    var divNode = document.createElement("div");
    divNode.setAttribute("class", "radio");

    // create <label class="radio-inline">
    var labelNode = document.createElement("label");
    labelNode.setAttribute("class", "input-group");
    
    // create radio button node
    /*var radioNode = document.createElement("input");
    radioNode.setAttribute("type", "radio");
    radioNode.setAttribute("name", "question-" + questionId + "-option");
    radioNode.setAttribute("value", "option-" + optionId);*/
    
    // create option text node
    var optionTextNode = document.createElement("input");
    optionTextNode.setAttribute("type", "text");
    optionTextNode.setAttribute("placeholder", "Ovdje unesi odgovor!");
    optionTextNode.setAttribute("size", "65");
    optionTextNode.setAttribute("class", "form-control input-lg");

    // create remove option node
    var removeOptionNode = document.createElement("button");
    removeOptionNode.setAttribute("class", "btn btn-sm btn-outline-danger");
    removeOptionNode.innerHTML = '<i class="fa fa-times"></i>';

    //removeOptionNode.setAttribute("aria-hidden", "true");
    removeOptionNode.addEventListener("click", function () {
        removeOption(this);
    });
   

    // attach to labelNode
    //labelNode.appendChild(radioNode);
    
    labelNode.appendChild(optionTextNode);
    labelNode.appendChild(removeOptionNode);

    //labelNode.appendChild(createNewLineNode());

    // attach to divNode
    divNode.appendChild(labelNode);

    // attach to tdNode
    tdNode.appendChild(divNode);
    //tdNode.appendChild(document.createTextNode("\u00A0"));

    // attach to trNode
    trNode.appendChild(tdNode);

    document.body.appendChild(trNode);

    return trNode;
}

function createQuestionNode(questionId) {
    /*
      HTML Format :
      <tbody id="question-0-section">
        <tr>
          <td colspan="2">
            <input type="text" name="question-{questionId}-text" placeholder="Type your question here!" size="90" class="form-control input-lg"> <br/>
          </td>
        </tr>
      </tbody>
     */
    var tbodyNode = document.createElement("tbody");
    tbodyNode.setAttribute("id", "question-"+questionId+"-section");

    // create tr node
    var trNode = document.createElement("tr");

    // create td node
    var tdNode = document.createElement("td");
    tdNode.setAttribute("colspan", "2");

    // create question text node
    var questionTextNode = document.createElement("input");
    questionTextNode.setAttribute("type", "text");
    questionTextNode.setAttribute("name", "question-"+questionId +"-text");
    questionTextNode.setAttribute("placeholder", "Unesi pitanje");
    questionTextNode.setAttribute("size", "100");
    questionTextNode.setAttribute("maxlength", "100");
    questionTextNode.setAttribute("class", "form-control form-control-lg");
    questionTextNode.setAttribute("style", "height:80px; font-size:40px");

    tdNode.appendChild(questionTextNode);
    trNode.appendChild(tdNode);

    var trNode2 = document.createElement("tr");

    var tdLabel = document.createElement("td");
    var label = document.createElement("label");
    label.innerHTML = "Vrsta pitanja:";

    tdLabel.appendChild(label);
    trNode2.appendChild(tdLabel);

    // create td for question type
    var tdNode2 = document.createElement("td");


    //create select
    var selectNode = document.createElement("select");
    selectNode.setAttribute("name", "question-"+questionId+"-type");
    selectNode.setAttribute("class", "form-control");
    //selectNode.setAttribute("class", "form-control");
    selectNode.setAttribute("id", "question-"+questionId+"-type");
    var optionOne = document.createElement("option");
    optionOne.setAttribute("value", "one");
    optionOne.innerHTML = "Odaberi jedan odgovor";
    var optionMult = document.createElement("option");
    optionMult.setAttribute("value", "multiple");
    optionMult.innerHTML = "Odaberi više odgovora";

    selectNode.appendChild(optionOne);
    selectNode.appendChild(optionMult);
    tdNode2.appendChild(selectNode);
    trNode2.appendChild(tdNode2);



    tbodyNode.appendChild(trNode2);
    tbodyNode.appendChild(trNode);


    return tbodyNode;

}

function isBlank(str) {
    return (!str || /^\s*$/.test(str));
}

function getQuestionTextNode(questionId) {
    return document.querySelector("#question-" + questionId + "-section > tr:nth-child(2) > td > input");
}

function getOptionTextNodes(questionId) {
    return document.querySelectorAll("#question-" + questionId + "-section > tr > td > div > label > input.form-control.input-lg");
}

function getOptionRadioNodes(questionId) {
    return document.querySelectorAll("#question-" + questionId + "-section > tr > td > div > label > input[type=\"radio\"]:nth-child(1)");
}
function getQuestionType(questionId) {
    return document.getElementsByName("question-" + questionId + "-type")[0].value;
}

function checkValidity(questionId) {


    // assert question text not blank
    var questionTextNode = getQuestionTextNode(questionId);
    if (isBlank(questionTextNode.value)) {
        return "invalid";
    }

    var index;
    // assert all option text not blank
    var optionNodes = getOptionTextNodes(questionId);
    for (index = 0; index < optionNodes.length; index++) {
        var optionNode = optionNodes[index];
        if (isBlank(optionNode.value)) {
            return "invalid";
        }
    }

    

    // assert exactly one checked
    return "valid";
}

function updateStatus(questionId, status) {
    var statusNode = document.getElementById("status-" + questionId);
    statusNode.setAttribute("class", statusClass[status]);
}

function setCurrentQuestionTo(questionId) {
    if (currentQuestion !== -1) {
        // Update status bar
        updateStatus(currentQuestion, checkValidity(currentQuestion));
        // Hide current question
        var currentQuestionNode = document.getElementById("question-" + currentQuestion + "-section");
        currentQuestionNode.setAttribute("hidden", "");
    }

    // Change current question
    currentQuestion = questionId;
    // Update title with current question
    document.getElementById("current-question-title").innerHTML = "";
    // Remove hidden attribute from current question
    var newCurrentQuestionNode = document.getElementById("question-" + currentQuestion + "-section");
    newCurrentQuestionNode.removeAttribute("hidden");

    updateStatus(questionId, "current");
}

function changeQuestionTo(target) {
    var questionId = parseInt(target.innerHTML) - 1;
    setCurrentQuestionTo(questionId);
}

function addToStatusBar(questionId) {
    var statusContainerNode = document.getElementById("question-status");
    var statusNode = document.createElement("button");
    statusNode.setAttribute("type", "button");
    statusNode.setAttribute("class", statusClass["current"]);
    statusNode.setAttribute("id", "status-" + questionId);
    statusNode.addEventListener("click", function () {
        changeQuestionTo(this);
    });
    statusNode.innerHTML = "1";
    statusNode.style.visibility = "hidden";
    statusContainerNode.appendChild(statusNode);
    statusContainerNode.appendChild(document.createTextNode("\u00A0"));
}

function addOption() {
    var tbodyNode = document.getElementById("question-" + currentQuestion + "-section");
    var newOptionId = numberOfOptionList[currentQuestion]++;
    tbodyNode.appendChild(createOptionNode(currentQuestion, newOptionId));
}

function addQuestion() {
    if (numberOfQuestion >= 1) return;
    var controlNode = document.getElementById("quiz-control");
    var tableNode = controlNode.parentNode;
    var newQuestionId = numberOfQuestion++;

    numberOfOptionList.push(0);

    tableNode.insertBefore(createQuestionNode(newQuestionId), controlNode);
    addToStatusBar(newQuestionId);

    setCurrentQuestionTo(newQuestionId);

    // Add three options
    addOption();
    addOption();
    addOption();

    if (numberOfQuestion === 1) {
        //document.querySelector("#quiz-control > tr > td.text-right > button.btn.btn-warning.btn-lg").setAttribute("disabled", "");
    }
}

function removeOption(target) {
    var optionNode = target.parentNode.parentNode;
    var parentNode = optionNode.parentNode.parentNode;
    parentNode.remove();
}

function saveQuiz() {
    var questionId;
    var questionsData = [];

    // Iterate over all questions
    for (questionId = 0; questionId < numberOfQuestion; questionId++) {
        // Check if valid
        var questionType = getQuestionType(questionId);

        if (checkValidity(questionId) === "invalid") {
            alert("Neka pitanja nisu dobro zadana. Provjeri imaju li sva ispravne odgovore i jesu li sva polja unesena!");
            return false;
        }
        var questionText = getQuestionTextNode(questionId).value;
        var optionsText = [];

        var optionTextNodes = getOptionTextNodes(questionId);
        var index = 0;
        for (index = 0; index < optionTextNodes.length; index++) {
            var optionTextNode = optionTextNodes[index];
            optionsText.push(optionTextNode.value);
        }

        
        questionsData = {
            "pitanje": questionText,
            "odgovori": optionsText,
            "vrsta": questionType
        };
    }
    document.getElementById("testic").innerHTML = JSON.stringify(questionsData);
    // store this to local storage
    storeToStorage(questionsData);
    return true;
}

function redirectToPlay() {
    document.getElementById("formy").submit();
    // window.location = "/Kviz/Rijesi"
}

function addQuizControlEventListener() {
    // Add Option Button
    document.getElementById("dodaj").addEventListener("click", function () {
        addOption();
    });

    // Add Question Button
    /*document.querySelector("#quiz-control > tr > td.text-right > button.btn.btn-warning.btn-lg").addEventListener("click", function () {
        addQuestion();
    });*/

    // Add Start Quiz Button

    document.getElementById("predaj").addEventListener("click", function () {
        saveQuiz() && redirectToPlay();
    });
}

(function () {

    addQuestion();
    addQuizControlEventListener();
})();