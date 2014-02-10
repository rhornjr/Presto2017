var stack = [];

$(function () {
    $('#waitLoadList').hide();

    $('#appsLink').click(showApps);
    $('#serversLink').click(showServers);
    $('#variablesLink').click(showVariableGroups);
    $('#installsLink').click(showInstalls);

    showApps();
});

function showApps() {    
    hideAll();
    $('#appList').show();
    $('#prestoTopic').text('Apps');
}

function showServers() {
    hideAll();
    $('#serverList').show();
    $('#prestoTopic').text('Servers');
}

function showVariableGroups() {
    hideAll();
    $('#variableGroupList').show();
    $('#prestoTopic').text('Variable Groups');
}

function showInstalls() {
    hideAll();
    $('#installsList').show();
    $('#prestoTopic').text('Installations');
}

function showApp(id) {
    hideDetail();
    $('#app').show();
    window.appInitialize(id);
    stack.push('appList');  // What goes here is the div we want to display when the user hits the back button.
}

function showServer(id) {
    hideDetail();
    $('#server').show();
    window.serverInitialize(id);
    stack.push('serverList');  // What goes here is the div we want to display when the user hits the back button.
}

function hideAll() {
    hideTopic();
    hideDetail();
}

function hideTopic() {
    $('#appList').hide();
    $('#serverList').hide();
    $('#variableGroupList').hide();
    $('#installsList').hide();
}

function hideDetail() {
    $('#app').hide();
    $('#server').hide();
    $('#task0').hide();
    $('#task1').hide();
    $('#task2').hide();
    $('#task3').hide();
}

function showTask(app, task) {
    hideDetail();

    stack.push('app');

    var taskDiv = $('#task' + task.PrestoTaskType);
    if (taskDiv[0]) {
        taskDiv.show();
        window.taskInitialize(app, task);
        return;
    }

    alert('No task view available for task type ' + task.PrestoTaskType);
}