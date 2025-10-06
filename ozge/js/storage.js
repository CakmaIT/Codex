(function () {
  if (!('indexedDB' in window)) {
    window.ContentDB = {
      async listFiles() {
        return [];
      },
      async getFile() {
        return null;
      },
      async deleteFile() {},
      async storeImportedFile() {
        throw new Error('IndexedDB not supported');
      }
    };
    return;
  }

  const DB_NAME = 'ozge-content-store';
  const DB_VERSION = 1;
  const STORE_FILES = 'files';
  let dbPromise = null;

  function openDb() {
    if (dbPromise) return dbPromise;
    dbPromise = new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);
      request.onupgradeneeded = () => {
        const db = request.result;
        if (!db.objectStoreNames.contains(STORE_FILES)) {
          const store = db.createObjectStore(STORE_FILES, { keyPath: 'id' });
          store.createIndex('classId', 'classId', { unique: false });
          store.createIndex('addedAt', 'addedAt', { unique: false });
        }
      };
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
    return dbPromise;
  }

  async function withStore(mode, handler) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_FILES, mode);
      const store = tx.objectStore(STORE_FILES);
      handler(store, tx);
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
    });
  }

  async function saveFile(record) {
    await withStore('readwrite', (store) => {
      store.put(record);
    });
    return record.id;
  }

  async function listFiles(classId) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_FILES, 'readonly');
      const store = tx.objectStore(STORE_FILES);
      const index = store.index('classId');
      const request = index.getAll(IDBKeyRange.only(classId));
      request.onsuccess = () => {
        const files = (request.result || []).sort((a, b) => {
          return new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime();
        });
        resolve(files);
      };
      request.onerror = () => reject(request.error);
    });
  }

  async function deleteFile(id) {
    await withStore('readwrite', (store) => {
      store.delete(id);
    });
  }

  async function getFile(id) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(STORE_FILES, 'readonly');
      const store = tx.objectStore(STORE_FILES);
      const request = store.get(id);
      request.onsuccess = () => resolve(request.result || null);
      request.onerror = () => reject(request.error);
    });
  }

  async function storeImportedFile(classId, file, extras = {}) {
    const buffer = await file.arrayBuffer();
    const blob = new Blob([buffer], { type: file.type || 'application/octet-stream' });
    const id = typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : `file-${Date.now()}-${Math.random()}`;
    const record = {
      id,
      classId,
      name: file.name,
      type: file.type || 'application/octet-stream',
      size: file.size,
      addedAt: new Date().toISOString(),
      blob,
      ...extras
    };
    await saveFile(record);
    return record.id;
  }

  window.ContentDB = {
    listFiles,
    getFile,
    deleteFile,
    storeImportedFile
  };
})();
