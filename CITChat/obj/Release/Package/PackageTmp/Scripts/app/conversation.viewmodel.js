window.CITChatApp.conversationViewModel = (function(ko, datacontext) {
    /// <field name="conversations" value="[new datacontext.conversation()]"></field>
    var conversations = ko.observableArray();
    var allUsers = ko.observableArray();
    var error = ko.observable();
    //var hasConversation = function(conversationId) {
    //    return true;
    //};
    var addConversation = function() {
        var conversation = datacontext.createConversation();
        conversation.isEditingConversationTitle(true);
        datacontext.saveNewConversation(conversation)
            .then(addSucceeded)
            .fail(addFailed);
        function addSucceeded() {
            showConversation(conversation);
            datacontext.conversationChanged(conversation.conversationId);
        }
        function addFailed() {
            error("Save of new conversation failed");
        }
    };
    var showConversation = function(conversation) {
        conversations.unshift(conversation); // Insert new conversation at the front
    };
    var deleteConversation = function(conversation) {
            conversations.remove(conversation);
        datacontext.deleteConversation(conversation)
            .then(deleteSucceeded)
            .fail(deleteFailed);
        function deleteSucceeded() {
            datacontext.conversationChanged(conversation.conversationId);
        }
        function deleteFailed() {
                showConversation(conversation); // re-show the restored list
            }
    };
    datacontext.getConversations(conversations, error); // load conversations
    datacontext.getAllUsers(allUsers, error); // load all users
    return {
        conversations: conversations,
        allUsers: allUsers,
        getOtherUsers: getOtherUsers,
        error: error,
        addConversation: addConversation,
        deleteConversation: deleteConversation
    };
    
    function getOtherUsers(conversation) {
        //    function containsUser(user) {
        //        var found = false;
        //        conversation.users.forEach(function (u2) { if (user.Id === u2.Id) { found = true; } });
        //        return !found;
        //    }
        //var otherUsers = ko.utils.arrayFilter(allUsers,
        //    function(u) {
        //        return !containsUser(u);
        //    });
        //return otherUsers;
        return allUsers;
    }

})(ko, CITChatApp.datacontext);

// Initiate the Knockout bindings
ko.applyBindings(window.CITChatApp.conversationViewModel);