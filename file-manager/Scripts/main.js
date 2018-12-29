
$(function () {

    initializeJqueryUi();

    var addressSegments = location.pathname.split('/');
    var searchParams = new URLSearchParams(document.location.search);
    var searchText = searchParams.get("query") === null ? '' : searchParams.get("query");

    if (addressSegments[1].toLowerCase() == 'browse') {
        addressSegments.splice(0, 2);
    } else {
        history.pushState(null, null, location.origin + '/browse/');
        addressSegments = [];
        searchText = '';
    }
    
    browserModel = new brwsrMdl(addressSegments, searchText);
    transferModel = new trnsfrMdl();
    ko.applyBindings(browserModel, document.getElementById("directory-browser-dialog"));
    ko.applyBindings(browserModel, document.getElementById("delete-dialog"));
    ko.applyBindings(transferModel, document.getElementById("transfer-dialog"));

})

var baseModel = function (_addressSegments, _searchText) {

    var self = this;
    self.modelName = ko.observable();
    self.currentPath = ko.observableArray(_addressSegments);
    self.queryType = ko.observable();
    self.searchText = ko.observable(_searchText);
    self.dirPath = ko.observable();
    self.dirFolderCount = ko.observable();
    self.dirFileCount = ko.observable();
    self.dirSize = ko.observable();
    self.dirItems = ko.observableArray();
    self.status = ko.observable();
    self.userMessage = ko.observable();
    self.pushHistory = ko.observable(true);
    self.selectionObject = {
        destination: ko.observable(),
        sources: ko.observableArray()
    }

    self.pathControlItem = function (index) {
        self.currentPath(self.currentPath().slice(0, index + 1));
        self.apiGetItems();
    }

    self.directoryItem = function (data, event) {
        self.currentPath.push(data.Name);
        self.apiGetItems('', function () { self.currentPath.pop(); });
    }

    self.searchPathItem = function (data, event) {
        self.currentPath(data.ItemPath.split('/'));
        self.apiGetItems();
    }

    self.searchQuery = function () {
        if (self.queryType() == 'directories') {
            return '?directoriesOnly=true';
        } else {
            return self.searchText().length > 0 ? '?query=' + self.searchText() : '';
        }
    }

    self.pushHistoryState = function () {
        history.pushState(self.currentPath(), null, location.origin + '/browse/' + self.currentPath().join('/') + self.searchQuery());
    }

    self.apiGetItems = function (message, errorHandler) {

        $.getJSON(location.origin + '/api/' + self.currentPath().join('/') + self.searchQuery(), function (data) {
            if (self.currentPath().length == 0) { self.currentPath.push(data.Name); }
            if (self.pushHistory()) { self.pushHistoryState(); }
            self.dirPath(data.ItemPath);
            self.dirFolderCount(data.FolderCount);
            self.dirFileCount(data.FileCount);
            self.dirSize(data.Size);
            self.dirItems(data.Items);
            self.status('ready');
            self.userMessage(typeof message === 'undefined' ? '' : message);
        })
            .fail(function (jqXHR) {
                if (errorHandler) { errorHandler(); }
                self.userMessage(jqXHR.statusText);
            })
            .always(function () {                         
                if (self.modelName() == 'browser') {
                    self.searchText('');
                    self.selectionObject.sources([]);
                    self.pushHistory(true);
                }
            });
    }

    self.apiTransferItems = function (transferType) {
       
        $.ajax({
            type: 'POST',
            url: location.origin + '/api/' + transferType + 'Items/',
            data: ko.toJSON(self.selectionObject),
            contentType: 'application/json; charset=utf-8',
            dataType: 'text'
        })
          .done(function (data, textStatus, jqXHR) {
              self.queryType(null);
              self.apiGetItems(jqXHR.statusText);
          })
          .fail(function (jqXHR, textStatus, errorThrown) {
              self.userMessage(jqXHR.statusText);
          })
          .always(function () {
              self.selectionObject.sources([]);
          });
    }

    self.apiDeleteItems = function () {

        $.ajax({
            type: 'DELETE',
            url: location.origin + '/api/DeleteItems/',
            data: ko.toJSON(self.selectionObject),
            contentType: 'application/json; charset=utf-8',
            dataType: 'text'
        })
          .done(function (data, textStatus, jqXHR) {
              self.apiGetItems(jqXHR.statusText);
          })
          .fail(function (jqXHR, textStatus, errorThrown) {
              self.userMessage(jqXHR.statusText);
          })
          .always(function () {
              self.selectionObject.sources([]);
          });
    }
};

var browserModel;

var brwsrMdl = function (_addressSegments, _searchText) {

    var self = this;

    ko.utils.extend(self, new baseModel(_addressSegments, _searchText));

    self.modelName('browser');

    self.directoryInfo = ko.computed(function () {

        if (self.status() == 'ready') {
            var foldersText = self.dirFolderCount() == 1 ? ' folder, ' : ' folders, ';
            var filesText = self.dirFileCount() == 1 ? ' file (' : ' files (';

            return self.dirFolderCount() + foldersText +
            self.dirFileCount() + filesText +
            self.dirSize() + ')';
        }
    }, self);

    self.search = function (data, event) {
        if (event.keyCode == 13 || event.type == 'click') {
            event.preventDefault();
            if (self.searchText().trim().length > 0) {
                self.apiGetItems(self.searchMessage());
            }
        }
        return true;
    }

    self.searchItem = function (Name, filePath) {
        return ko.computed(function () {
            return location.origin + '/download/' + filePath + '?file=' + fileName;
        }, self);
    }

    self.searchMessage = function () {
        return "Search results for '" + self.searchText() + "'";
    }

    self.fileLink = function (fileName, filePath) {
        return ko.computed(function () {
            return location.origin + '/download/' + filePath + '?file=' + fileName;
        }, self);
    }

    self.upload = function (files) {

        var formdata = new FormData();

        for (i = 0; i < files.length; i++) {
            formdata.append(files[i].name, files[i]);
        }

        $.ajax({
            type: 'POST',
            url: location.origin + '/upload/' + self.currentPath().join('/'),
            data: formdata,
            processData: false,
            contentType: false 
        })
          .done(function (data, textStatus, jqXHR) {
              self.apiGetItems(data);
          })
          .fail(function (jqXHR, textStatus, errorThrown) {
              self.userMessage(jqXHR.statusText);
          })
    }

    self.openTransferDialog = function (type) {
        if (self.selectionObject.sources().length > 0) {
            transferModel.selectionObject.sources(self.selectionObject.sources());
            transferModel.transferType(type);
            transferModel.apiGetItems('Navigate to a directory to ' + type.toLowerCase() + ' the selected items to.');
            $('#transfer-dialog').dialog('open');            
        } else {
            self.userMessage('Please select items to ' + type.toLowerCase());
        }
    }

    self.openDeleteDialog = function () {
        if (self.selectionObject.sources().length > 0) {
            $('#delete-dialog').dialog('open');
        } else {
            self.userMessage('Please select items to delete.');
        }
    }

    self.deleteItems = function () {
        if (self.selectionObject.sources().length > 0) {
            $('#delete-dialog').dialog('close');
            self.apiDeleteItems();
        }
    }

    self.closeDeleteDialog = function () {
        $('#delete-dialog').dialog('close');
        self.selectionObject.sources([]);
    }

    ko.utils.registerEventHandler(window, "popstate", function (event) {
        if (window.history.state) {
            self.currentPath(window.history.state);
            if (self.currentPath().length > 1) { self.pushHistory(false); }
        }

        self.apiGetItems();
    });

    self.apiGetItems(self.searchText().length > 0 ? self.searchMessage() : '');

};

var transferModel;

var trnsfrMdl = function () {

    var self = this;

    ko.utils.extend(self, new baseModel([], ''));

    self.modelName('transfer');
    self.transferType = ko.observable('');
    self.pushHistory(false);
    self.queryType('directories');

    self.transfer = function (type) {
        if (self.selectionObject.sources().length > 0) {
            self.selectionObject.destination(self.currentPath().join('\\'));
            self.apiTransferItems(self.transferType());
        }
    }

    self.closeTransferDialog = function () {
        self.queryType('directories');
        self.currentPath([]);
        browserModel.apiGetItems();
        $('#transfer-dialog').dialog('close');
    }
};

function initializeJqueryUi() {

    $("#directory-browser-dialog").dialog({ maxHeight: 800, minHeight: 800, width: 1000, autoOpen: false });
   
    $('#transfer-dialog').dialog({ maxHeight: 700, minHeight: 700, width: 900, autoOpen: false, modal: true, dialogClass: 'dialog-close-not-visible' });

    $("#delete-dialog").dialog({ autoOpen: false, resizable: false, height: "auto", width: 400, modal: true });

    $("#open-directory-browser").on("click", function () { $("#directory-browser-dialog").dialog("open"); });

}