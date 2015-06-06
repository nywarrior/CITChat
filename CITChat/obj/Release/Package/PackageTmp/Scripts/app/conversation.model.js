(function(ko, datacontext) {
    datacontext.message = message;
    datacontext.user = user;
    datacontext.conversation = conversation;
    datacontext.addUserRequest = addUserRequest;

    function user(data) {
        var self = this;
        data = data || {};
        // Persisted properties
        self.userId = data.userId;
        self.userName = ko.observable(data.userName);
        // Non-persisted properties
        self.errorMessage = ko.observable();
        var saveChanges = function() {
            return datacontext.saveChangedUser(self);
        };
        // Auto-save when these properties change
        self.userName.subscribe(saveChanges);
        self.toJson = function() { return ko.toJSON(self); };
    }

    function message(data) {
        var self = this;
        data = data || {};
        // Persisted properties
        self.messageId = data.messageId;
        self.content = ko.observable(data.content);
        self.translatedContent = ko.observable(data.translatedContent);
        self.isDone = ko.observable(data.isDone);
        self.conversationId = data.conversationId;
        // Non-persisted properties
        self.errorMessage = ko.observable();
        var saveChanges = function() {
            return datacontext.saveChangedMessage(self);
        };
        // Auto-save when these properties change
        self.isDone.subscribe(saveChanges);
        self.content.subscribe(saveChanges);
        self.translatedContent.subscribe(saveChanges);
        self.toJson = function() { return ko.toJSON(self); };
    }

    function addUserRequest(data) {
        var self = this;
        data = data || {};
        // Persisted properties
        self.userId = data.userId;
        self.conversationId = data.conversationId;
        // Non-persisted properties
        self.errorMessage = ko.observable();
        self.toJson = function () { return ko.toJSON(self); };
    }
    
    function conversation(data) {
        var self = this;
        data = data || {};
        // Persisted properties
        self.conversationId = data.conversationId;
        self.title = ko.observable(data.title || "New Conversation");
        self.startDateTime = ko.observable(data.startDateTime || new Date());
        self.startDateTimeDisplayString = ko.observable(data.startDateTimeDisplayString || new Date());
        self.messages = ko.observableArray(importMessages(data.messages));
        self.users = ko.observableArray(importUsers(data.users));
        // Non-persisted properties
        self.isEditingConversationTitle = ko.observable(false);
        self.newMessageContent = ko.observable();
        self.errorMessage = ko.observable();
        self.deleteMessage = function() {
            var message = this;
            return datacontext.deleteMessage(conversation2, message)
                .done(function() { self.messages.remove(message); });
        };
        // Auto-save when these properties change
        self.title.subscribe(function() {
            return datacontext.saveChangedConversation(self);
        });
        self.startDateTime.subscribe(function() {
            return datacontext.saveChangedConversation(self);
        });
        self.startDateTimeDisplayString.subscribe(function() {
            return datacontext.saveChangedConversation(self);
        });
        self.messages.subscribe(function() {
            return datacontext.saveChangedConversation(self);
        });
        self.toJson = function() { return ko.toJSON(self); };
    }

    // convert raw message data objects into array of Messages

    function importMessages(messages) {
        /// <returns value="[new message()]"></returns>
        return $.map(messages || [],
            function(messageData) {
                return datacontext.createMessage(messageData);
            });
    }

    function importUsers(users) {
        /// <returns value="[new user()]"></returns>
        return $.map(users || [],
            function(userData) {
                return datacontext.createUser(userData);
            });
    }

    conversation.prototype.addMessage = function() {
        var self = this;
        if (self.newMessageContent()) { // need content to save
            var newMessage = datacontext.createMessage(
                {
                    content: self.newMessageContent(),
                    conversationId: self.conversationId
                });
            var successCallback = function () {
                self.messages.push(newMessage);
            };
            var conversation2 = this;
            datacontext.saveNewMessage(conversation2, newMessage)
                .then(successCallback);
            self.newMessageContent("");
        }
    };

    conversation.prototype.addUserToConversation = function () {
        var self = this;
        var userId = $("#userToAdd").val();
        var newAddUserRequest = datacontext.createAddUserRequest(
                {
                    userId: userId,
                    conversationId: self.conversationId
                });
        var successCallback = function () {
            var newAddedUser = datacontext.createUser(
                {
                    userId: userId,
                    userName: newAddUserRequest.userName
                });
            self.users.push(newAddedUser);
        };
        var conversation2 = self;
        datacontext.saveAddUserRequest(conversation2, newAddUserRequest)
                .then(successCallback);
    };
})(ko, CITChatApp.datacontext);