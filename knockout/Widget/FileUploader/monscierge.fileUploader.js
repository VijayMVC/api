ko.bindingHandlers.fileUploader = {
    'init': function (element, valueAccessor) {
    },
    'update': function (element, valueAccessor) {
        var vm = valueAccessor();
        $(element).fileupload({
            add: vm.Add,
            submit: vm.Submit,
            send: vm.Send,
            done: vm.Done,
            fail: vm.Fail,
            always: vm.Always,
            progress: vm.Progress,
            progressAll: vm.ProgressAll,
            start: vm.Start,
            stop: vm.Stop,
            change: vm.Change,
            paste: vm.Paste,
            drop: vm.Drop,
            dragOver: vm.DragOver,
            chunkSend: vm.ChuckSend,
            chunkDone: vm.ChunkDone,
            chunkFail: vm.ChunkFail,
            chunkAlways: vm.ChunckAlways,
            url: vm.UploadUrl,
            dropZone: vm.DropZone,
            pasteZone: vm.PasteZone,
            type: vm.type,
			datatype: vm.datatype
        });
    }
}