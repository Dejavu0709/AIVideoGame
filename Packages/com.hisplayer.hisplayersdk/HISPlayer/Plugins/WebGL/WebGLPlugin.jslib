mergeInto(LibraryManager.library, {

  // #region Core Player Controls
  HISPLAYERUnity_Init : function (url, serverKey) {
    setUpContext(GL);
  },

  HISPLAYERUnity_InitializeMultistream : function (hisPlayerKey, sdkVersion) {
    multiView.Initialize(UTF8ToString(hisPlayerKey), UTF8ToString(sdkVersion));
  },

  HISPLAYERUnity_ChangeVideoContent : function (url, resumePosition, ads) {
    return multiView.changeVideoContent(UTF8ToString(url), resumePosition, UTF8ToString(ads));
  },

  HISPLAYERUnity_Resume : function () {
    multiView.resume();
  },

  HISPLAYERUnity_Pause : function () {
    multiView.pause();
  },

  HISPLAYERUnity_Stop : function () {
    multiView.stopPlayer();
  },

  HISPLAYERUnity_Seek : function (milliseconds) {
    multiView.seek(milliseconds);
  },

  HISPLAYERUnity_Release : function () {
    multiView.release();
  },
  // #endregion

  // #region MultiStream Video Handling
  HISPLAYERUnity_SetMultistream : function () {
    if (window.multiView) {
      multiView.release();
    }
    multiView = new MultipleView();
  },

  HISPLAYERUnity_ChangeControlInstance : function (value) {
    multiView.changeControlInstance(value);
  },

  HISPLAYERUnity_SetMultistreamVolume : function (value) {
    multiView.setVolume(value);
  },

  HISPLAYERUnity_SetMultiPaths : function (
    index, path, autoPlayback, loopPlay, disableABR, 
    manifestTimeout, segmentsTimeout, startingBitrate, 
    maxBitrate, minBitrate, maxWidth, maxHeight, 
    minWidth, minHeight) {

    multiView.setMultiPaths(index, UTF8ToString(path), 
    Boolean(autoPlayback), Boolean(loopPlay), Boolean(disableABR), 
    manifestTimeout, segmentsTimeout, startingBitrate, maxBitrate, 
    minBitrate, maxWidth, maxHeight, minWidth, minHeight);
  },

  HISPLAYERUnity_updateMultistreamVideoTexture : function (textureId, elementIndex, textureWidth, textureHeight) {
    return multiView.updateVideoTexture(GL.textures[textureId], elementIndex, GL.currentContext, textureWidth, textureHeight);
  },

  HISPLAYERUnity_GetMultistreamContentInfoInt : function (elementIndex, info_index) {
    return multiView.getContentInfo(elementIndex, info_index);
  },
  // #endregion

  // #region Tracks
  HISPLAYERUnity_GetTrackCount : function () {
    return multiView.getTrackCount();
  },

  HISPLAYERUnity_GetTrackID : function (index) {
    var idStr = multiView.getTrackID(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_GetTrackBitrate : function (index) {
    return multiView.getTrackBitrate(index);
  },

  HISPLAYERUnity_GetTrackWidth : function (index) {
    return multiView.getTrackWidth(index);
  },

  HISPLAYERUnity_GetTrackHeight : function (index) {
    return multiView.getTrackHeight(index);
  },

  HISPLAYERUnity_SetTrack : function (idx) {
    multiView.setCurrentTrack(idx);
  },

  // I don't know if this function is been used, is not defined in HISPlayerWebGL.cs
  HISPLAYERUnity_GetCurrentTrackID : function (index) {
    return multiView.getCurrentTrackID();
  },
  // #endregion

  // #region Captions
  HISPLAYERUnity_GetCaptionCount : function () {
    return multiView.getCaptionCount();
  },

  HISPLAYERUnity_GetCaptionTrackID : function (index) {
    var idStr = multiView.getCaptionTrackID(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_GetCaptionTrackLanguage : function (index) {
    var idStr = multiView.getCaptionTrackLanguage(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_EnableCaptions : function () {
    multiView.enableCaptions();
  },

  HISPLAYERUnity_DisableCaptions : function () {
    multiView.disableCaptions();
  },

  HISPLAYERUnity_SetCaptionTrack : function (index) {
    multiView.setCaptionTrack(index);
  },
  // #endregion

  // #region Audio
  HISPLAYERUnity_GetAudioCount : function () {
    return multiView.getAudioCount();
  },

  HISPLAYERUnity_GetAudioTrackID : function (index) {
    var idStr = multiView.getAudioTrackID(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_GetAudioTrackLanguage : function (index) {
    var idStr = multiView.getAudioTrackLanguage(index);
    var bufferSize = lengthBytesUTF8(idStr)+1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(idStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_SetAudioTrack : function (index) {
    multiView.setAudioTrack(index);
  },
  // #endregion

  // #region Playback Info & Settings
  HISPLAYERUnity_GetCurrentTime : function () {
    return multiView.getCurrentTime();
  },

  HISPLAYERUnity_GetDuration : function () {
    return multiView.getDuration();
  },

  HISPLAYERUnity_IsLive : function () {
    return multiView.isLive();
  },

  HISPLAYERUnity_IsFirefox : function () {
    return navigator.userAgent.search("Firefox") > -1;
  },

  HISPLAYERUnity_GetPlayerStatus : function() {
    return multiView.getPlayerStatus();
  },

  HISPLAYERUnity_GetPlaybackSpeedRate : function() {
    return multiView.getPlaybackSpeedRate();
  },

  HISPLAYERUnity_SetPlaybackSpeedRate : function (speed) {
    multiView.setPlaybackSpeedRate(speed);
  },

  HISPLAYERUnity_GetPlaybackDurationLimit : function() {
    return getPlaybackDurationLimit();
  },

  HISPLAYERUnity_GetIsDurationLimitReady : function() {
    return getIsDurationLimitReady();
  },
  // #endregion

  // #region ABR
  HISPLAYERUnity_SetMaxAbrResolution : function (width, height) {
    multiView.setMaxAbrResolution(width, height);
  },

  HISPLAYERUnity_SetMinAbrResolution : function (width, height) {
    multiView.setMinAbrResolution(width, height);
  },

  HISPLAYERUnity_EnableABR : function () {
    multiView.enableABR();
  },

  HISPLAYERUnity_DisableABR : function () {
    multiView.disableABR();
  },
  // #endregion

  // #region Events
  HISPLAYERUnity_EventQueueIsEmpty : function () {
    return isEventQueueEmpty();
  },

  HISPLAYERUnity_GetEventType : function() {
    return getEventType();
  },
  
  HISPLAYERUnity_GetJsonFormattedEvent : function() {
    var returnStr = getEventInfo();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  
  HISPLAYERUnity_GetEventPlayerIndex : function() {
    return getEventPlayerIndex();
  },
  // #endregion

  // #region Errors
  HISPLAYERUnity_ErrorQueueIsEmpty : function () {
    return isErrorQueueEmpty();
  },
  
  HISPLAYERUnity_GetErrorType : function() {
    return getErrorType();
  },

  HISPLAYERUnity_GetJsonFormattedError : function() {
    var returnStr = getErrorInfo();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  HISPLAYERUnity_GetErrorPlayerIndex : function() {
    return getErrorPlayerIndex();
  },
  // #endregion

  // #region Ad Insertion
  HISPLAYERUnity_GetAdDuration : function () {
    return multiView.getAdDuration();
  },

  HISPLAYERUnity_GetAdRemainingTime : function () {
    return multiView.getAdRemainingTime();
  },

  HISPLAYERUnity_GetAdCurrentTime : function () {
    return multiView.getAdCurrentTime();
  },

  HISPLAYERUnity_SetMultiAdsProperties : function (
    index, adsMode, assetKey, contentSrcId, videoId, baseUrl, manifestUrl,
    adsParams, adTagUri, enableVpaid, pageUrl) {

    multiView.setMultiAdsProperties(
      index, UTF8ToString(adsMode), UTF8ToString(assetKey), UTF8ToString(contentSrcId),
      UTF8ToString(videoId), UTF8ToString(baseUrl), UTF8ToString(manifestUrl),
      UTF8ToString(adsParams), UTF8ToString(adTagUri), Boolean(enableVpaid), UTF8ToString(pageUrl));
  },
  // #endregion

  // #region License
  HISPLAYERUnity_CheckWatermark : function (cb) {
    multiView.checkWaterMark().then(function (watermark) {
      dynCall_vi(cb, watermark);
    });    
  },

  HISPLAYERUnity_CheckLicenseError : function (cb) {
    multiView.checkLicenseError().then(function (errorCode) {
      dynCall_vi(cb, errorCode);
    });    
  },
  // #endregion

  // #region Logger
  HISPLAYERUnity_Log : function (level, message) {
    Logger.getInstance().log(level, UTF8ToString(message));
  },

  HISPLAYERUnity_SetLogLevel : function (level) {
    Logger.getInstance().setLogLevel(level);
  },
  // #endregion

});
