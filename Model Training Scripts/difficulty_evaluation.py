# Produces an evaluation of the game's current difficulty based on player performance
def evaluate_entry():

    # Import libraries
    import xgboost as xgb
    import UnityEngine
    import pickle
    import os

    # Retrieve the data of the current session
    evaluationManager = UnityEngine.GameObject.FindWithTag("Evaluation Manager").GetComponent("EvaluationManager")
    sessionData = evaluationManager.sessionData

    # Open the evaluation model
    model_path = os.path.join(UnityEngine.Application.dataPath, 'StreamingAssets/evaluationModel.pkl')
    with open(model_path, 'rb') as model_file:
        xgBoost = pickle.load(model_file)
    
    # Produce the prediction
    features = list(sessionData.keys())
    entry = [[sessionData[feature] for feature in features]]

    prediction = xgBoost.predict(entry)[0]

    # Store the model
    with open(model_path, 'wb') as model_file:
        pickle.dump(xgBoost, model_file)

    # Modify the difficulty according to the prediction
    if prediction == 0: evaluationManager.AdjustDifficulty("Decrease")
    elif prediction == 1: evaluationManager.AdjustDifficulty("Stay the same")
    elif prediction == 2: evaluationManager.AdjustDifficulty("Increase")