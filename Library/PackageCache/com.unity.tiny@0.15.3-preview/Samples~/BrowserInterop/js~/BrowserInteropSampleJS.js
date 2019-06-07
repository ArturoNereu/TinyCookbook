mergeInto(LibraryManager.library, {
    FetchRandomUserProfile: function () {
        var toCharArray = function(str) {
            var arr = [];
            for(var i = 0; i < str.length; i++) {
                var charCode = str.charCodeAt(i);
                if(charCode > 255)
                    return false;
                arr[i] = charCode;
            }
            return arr;
        }

        RandomUserSdk.fetchRandomUserProfile(function (userProfile) {
            var username = toCharArray(userProfile.name.first + " " + userProfile.name.last);
            if (!username)
                _FetchRandomUserProfile();

            var email = toCharArray(userProfile.email);
            var userId = toCharArray(userProfile.login.username + " (" + userProfile.login.uuid + ")");

            SendMessage("FetchRandomUserProfileMessage-UserName", [], [], username);
            SendMessage("FetchRandomUserProfileMessage-Email", [], [], email);
            SendMessage("FetchRandomUserProfileMessage-UserId", [], [], userId);
        });
    }
});
