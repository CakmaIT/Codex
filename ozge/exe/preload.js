const { contextBridge } = require('electron');

contextBridge.exposeInMainWorld('ozgeDesktop', {
  platform: process.platform
});
