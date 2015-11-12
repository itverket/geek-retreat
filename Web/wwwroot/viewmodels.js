var geek = geek || {};
geek.ajaxRequest = function (delegate) {
    if (typeof delegate === "function") {
        delegate = delegate();
    }
    delegate.type = delegate.type || "POST";
    delegate.dataType = delegate.dataType || "json";
    delegate.contentType = delegate.contentType || "application/json";
    delegate.traditional = delegate.traditional || true;

    $.ajax(delegate);
};
function TweetrSearchViewModel(self) {

    self.simpleSearch = function (tweet) {
        geek.ajaxRequest(self.simpleSearchSettings(tweet));
    };

    self.simpleSearchSettings = function (tweet) {
        return {
            url: "/api/search",
            data: JSON.stringify(self.getCriteriaAsModel(tweet)),
            success: self.onSearchSuccessFunction
        }
    };

    self.onSearchSuccessFunction = function (data) {
        // update resultlist with results from search.
        ko.viewmodel.updateFromModel(self.SearchResult.ResultList, data);
        self.Criteria.setPreviousSearchValues();
    };

    self.getCriteriaAsModel = function (tweet) {
        if (tweet && tweet.Document && tweet.Document.Username) {
            self.Criteria.Username(tweet.Document.Username());
        } else {
            self.Criteria.Username("");
        }
        var model = ko.viewmodel.toModel(self);

        return model.Criteria;
    };
}

function createTweetrSearchViewModel(model) {
    return ko.viewmodel.fromModel(model, {
        extend: {
            "{root}": TweetrSearchViewModel,
            "{root}.SearchResult": SearchResultViewModel,
            "{root}.SearchResult.ResultList[i]": TweetViewModel,
            "{root}.Criteria": SearchCriteriaViewModel
        }
    });
};

function SearchCriteriaViewModel(self) {
    self.Query.subscribe(function () {
        //do search on change?
        return "Query changed to " + self.Query();
    });

    self.previousSearchValues = ko.observable();

    self.setPreviousSearchValues = function () {
        self.previousSearchValues("Previous Query: '" + self.Query() + "', Username: '" + self.Username() + "'");
    }

    self.resetSearchValues = function () {
        self.Query("");
        self.Username("");
    }
}

function SearchResultViewModel(self) {
    
}

function TweetViewModel(self) {
    self.userNameClicked = function () {
        return "opprett metode i foreldremodel istedet!";
    };

    self.getTweetMessage = function () {
        return self.Document.TweetMessage();
    }

    self.getDocumentProperty = function (propertyName) {
        if (self.Document[propertyName]) {
            return self.Document[propertyName]();
        }
        return "";
    }

}