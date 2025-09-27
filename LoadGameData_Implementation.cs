// Replace the existing LoadGameData method with this implementation
bool LoadGameData()
{
    if (gameDataJson != null)
    {
        try
        {
            gameData = JsonUtility.FromJson<GameData>(gameDataJson.text);
            Debug.Log($"Loaded game data: {gameData.meta.title}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse game data JSON: {e.Message}");
            return false;
        }
    }
    else if (!string.IsNullOrEmpty(gameDataUrl))
    {
        StartCoroutine(LoadGameDataFromUrl());
        return true; // Return true as we've started the loading process
    }
    
    Debug.LogError("No game data source provided!");
    return false;
}

// Add this new coroutine method to your BranchingVideoGameManager class
private IEnumerator LoadGameDataFromUrl()
{
    Debug.Log($"Loading game data from URL: {gameDataUrl}");
    
    using (UnityWebRequest request = UnityWebRequest.Get(gameDataUrl))
    {
        // Send the request and wait for it to complete
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Parse the JSON data
                gameData = JsonUtility.FromJson<GameData>(request.downloadHandler.text);
                Debug.Log($"Successfully loaded game data from URL: {gameData.meta.title}");
                
                // Start the game after data is loaded
                StartGame();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse game data JSON from URL: {e.Message}");
                ShowErrorMessage("Failed to parse game data");
            }
        }
        else
        {
            Debug.LogError($"Failed to load game data from URL: {request.error}");
            ShowErrorMessage($"Failed to load game data: {request.error}");
        }
    }
}

// Add this helper method to show error messages to the user
private void ShowErrorMessage(string message)
{
    // Display error message to the user
    // You can implement this based on your UI system
    Debug.LogError(message);
    // Example: if (uiController != null) uiController.ShowError(message);
}
