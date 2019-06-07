RandomUserSdk = {
    fetchRandomUserProfile: function (callback) {
        var httpRequest = new XMLHttpRequest();
        httpRequest.onreadystatechange = function () {
            if (httpRequest.readyState === XMLHttpRequest.DONE) {
                if (httpRequest.status === 200) {
                    var jsonResponse = JSON.parse(httpRequest.response);
                    callback(jsonResponse.results[0]);
                } else {
                    console.error('There was a problem with the request.');
                }
            }
        };
        httpRequest.open('GET', 'https://randomuser.me/api/');
        httpRequest.send();
    }
};
