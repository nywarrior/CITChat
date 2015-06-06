window.CITChatApp = window.CITChatApp || {};

window.CITChatApp.datacontext = (function() {
    var datacontext = {
        getConversations: getConversations,
        getAllUsers: getAllUsers,
        getLoginUserName: getLoginUserName,
        createMessage: createMessage,
        createUser: createUser,
        createConversation: createConversation,
        createAddUserRequest: createAddUserRequest,
        saveAddUserRequest: saveAddUserRequest,
        saveNewMessage: saveNewMessage,
        saveNewConversation: saveNewConversation,
        saveChangedMessage: saveChangedMessage,
        saveChangedConversation: saveChangedConversation,
        deleteMessage: deleteMessage,
        deleteConversation: deleteConversation,
        conversationChanged: conversationChanged,
    };

    // Notify other clients through SignalR

    function conversationChanged(conversationId) {
        if ($.connection != null) {
            // Start the connection.
            // Declare a proxy to reference the hub. 
            var chatHub = $.connection.cITChatHub;
            if (chatHub != null) {
                // Call the Send method on the hub. 
                chatHub.server.sendConversationChanged(conversationId);
            }
            ;
        }
    }

    function getParameterByName(url, name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(url.search);
        return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    function getLoginUserName() {
        var loginUserName = getParameterByName(location, 'userName');
        if (loginUserName == "undefined") {
            loginUserName = null;
        }
        return loginUserName;
    }

    function isLoggedIn() {
        var loginUserName = getLoginUserName();
        var result = (loginUserName != "");
        return result;
    }

    function getConversations(conversationsObservable, errorObservable) {
        if (!isLoggedIn()) {
            conversationsObservable({});
            return null;
        }
        return ajaxRequest("get", conversationUrl())
            .done(getSucceeded)
            .fail(getFailed);

        function getSucceeded(data) {
            var conversations = $.map(data, function(c) { return new createConversation(c); });
            conversationsObservable(conversations);
        }

        function getFailed() {
            errorObservable("Error retrieving conversations.");
        }
    }

    function getAllUsers(allUsersObservable, errorObservable) {
        if (!isLoggedIn()) {
            allUsersObservable({});
            return null;
        }
        return ajaxRequest("get", userUrl())
            .done(getSucceeded)
            .fail(getFailed);

        function getSucceeded(data) {
            var allUsers = $.map(data, function(u) { return new createUser(u); });
            allUsersObservable(allUsers);
        }

        function getFailed() {
            errorObservable("Error retrieving all users.");
        }
    }

    function createMessage(data) {
        return new datacontext.message(data); // message is injected by conversation.model.js
    }

    function createUser(data) {
        return new datacontext.user(data); // user is injected by conversation.model.js
    }

    function createConversation(data) {
        return new datacontext.conversation(data); // conversation is injected by conversation.model.js
    }

    function createAddUserRequest(data) {
        return new datacontext.addUserRequest(data); // addUserRequest is injected by conversation.model.js
    }

    function saveNewMessage(conversationId, message) {
        clearErrorMessage(message);
        return ajaxRequest("post", messageUrl(), message)
            .done(function(result) {
                message.messageId = result.messageId;
                message.translatedContent = result.translatedContent;
                conversationChanged(conversationId);
            })
            .fail(function() {
                message.errorMessage("Error adding a new message.");
            });
    }

    function saveAddUserRequest(conversationId, addUserRequest) {
        clearErrorMessage(addUserRequest);
        return ajaxRequest("post", addUserRequestUrl(), addUserRequest)
            .done(function(result) {
                if (result != null) {
                    addUserRequest.userId = result.userId;
                    addUserRequest.userName = result.userName;
                    conversationChanged(conversationId);
                }
            })
            .fail(function() {
                addUserRequest.errorMessage("Error adding the selected user to the conversation.");
            });
    }

    function saveNewConversation(conversation) {
        clearErrorMessage(conversation);
        return ajaxRequest("post", conversationUrl("conversation"), conversation)
            .done(function(result) {
                conversation.conversationId = result.conversationId;
                conversation.users = result.users;
                conversation.messages = result.messages;
                conversation.startDateTime = result.startDateTime;
                conversation.startDateTimeDisplayString = result.startDateTimeDisplayString;
                conversationChanged(result.conversationId);
            })
            .fail(function() {
                conversation.errorMessage("Error adding a new conversation.");
            });
    }

    function deleteMessage(conversationId, message) {
        return ajaxRequest("delete", messageUrl(message.messageId))
            .done(function() {
                conversationChanged(conversationId);
            })
            .fail(function() {
                message.errorMessage("Error removing message.");
            });
    }

    function deleteConversation(conversation) {
        return ajaxRequest("delete", conversationUrl(conversation.conversationId))
            .done(function() {
                conversationChanged(conversation.ConversationId);
            })
            .fail(function() {
                conversation.errorMessage("Error removing conversation.");
            });
    }

    function saveChangedMessage(conversationId, message) {
        clearErrorMessage(message);
        return ajaxRequest("put", messageUrl(message.messageId), message, "text")
            .done(function() {
                conversationChanged(conversationId);
            })
            .fail(function() {
                message.errorMessage("Error updating message.");
            });
    }

    function saveChangedConversation(conversation) {
        clearErrorMessage(conversation);
        return ajaxRequest("put", conversationUrl(conversation.conversationId), conversation, "text")
            .done(function() {
                conversationChanged(conversation.conversationId);
            })
            .fail(function() {
                conversation.errorMessage("Error updating the conversation title. Please make sure it is non-empty.");
            });
    }

    // Private

    function clearErrorMessage(entity) {
        entity.errorMessage(null);
    }

    function ajaxRequest(type, url, data, dataType) { // Ajax helper
        var options = {
            dataType: dataType || "json",
            contentType: "application/json",
            cache: false,
            type: type,
            data: data ? data.toJson() : null
        };
        var antiForgeryToken = $("#antiForgeryToken").val();
        if (antiForgeryToken) {
            options.headers = {
                'RequestVerificationToken': antiForgeryToken
            };
        }
        return $.ajax(url, options);
    }

    // routes

    function conversationUrl(id) {
        var url = "/api/conversation/" + (id || "");
        var userName = getLoginUserName();
        if (userName != "") {
            url += "?UserName=" + userName;
        }
        return url;
    }

    function userUrl(id) {
        var url = "/api/user/" + (id || "");
        var userName = getLoginUserName();
        if (userName != "") {
            url += "?UserName=" + userName;
        }
        return url;
    }

    function messageUrl(id) {
        var url = "/api/message/" + (id || "");
        var userName = getLoginUserName();
        if (userName != "") {
            url += "?UserName=" + userName;
        }
        return url;
    }

    function addUserRequestUrl(id) {
        var url = "/api/addUserRequest/" + (id || "");
        var userName = getLoginUserName();
        if (userName != "") {
            url += "?UserName=" + userName;
        }
        return url;
    }

    return datacontext;

})();