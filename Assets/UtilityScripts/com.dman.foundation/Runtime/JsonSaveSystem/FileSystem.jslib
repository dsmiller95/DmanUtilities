var LibraryFileSystemWebGL = {


// sourced from UnityHub\<version>\Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\lib\FileSystem.js
// copied to ensure backwards and forwards compatibility
	
/* JS_FileSystem_Sync_state keeps track of the current filesystem sync status.
  It has the following values:
  0: no current filesystem sync in action,
  an integer value > 0: a setTimeout(0 msecs) handler is pending to start a
                        sync right after current frame.
  string value 'idb': An IndexedDB sync operation is currently in flight.
  string value 'again': An IndexedDB sync operation is currently in flight,
                        and when that finishes, a new sync operation should
                        start immediately after.
*/
$JS_FileSystem_Sync_state: '0',

JS_FileSystem_Sync__proxy: 'sync',
JS_FileSystem_Sync__sig: 'v',
JS_FileSystem_Sync__deps: ['$JS_FileSystem_Sync_state'],
JS_FileSystem_Sync: function()
{
	function onSyncComplete() {
		// IndexedDB sync operation has now finished. Check if we should
		// start a new sync round?
		if (JS_FileSystem_Sync_state === 'again') startSync();
		else JS_FileSystem_Sync_state = 0; // go idle, no more FS ops
	};

	function startSync() {
		JS_FileSystem_Sync_state = 'idb'; // We are now running an FS sync op
		FS.syncfs(/*readFromIDBToMemFS=*/false, onSyncComplete);
	};

	if (JS_FileSystem_Sync_state === 0) {
		// Programs typically write/copy/move multiple files in the in-memory
		// filesystem within a single app frame, so when a filesystem sync
		// command is triggered, do not start it immediately, but only after
		// the current frame is finished. This way all the modified files
		// inside the main loop tick will be batched up to the same sync.
		JS_FileSystem_Sync_state = setTimeout(startSync, 0);
	} else if (JS_FileSystem_Sync_state === 'idb') {
		// There is an active IndexedDB sync operation in-flight, but we now
		// have accumulated more files to sync. We should therefore immediately
		// start a new sync after the current one finishes so that all writes
		// will be properly persisted.
		JS_FileSystem_Sync_state = 'again';
	}
}

};

mergeInto(LibraryManager.library, LibraryFileSystemWebGL);
