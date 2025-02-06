# Import libraries
import numpy as np
import pandas as pd
import xgboost as xgb
import pickle

from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score
from sklearn.preprocessing import MinMaxScaler

difficulty_evaluation = pd.read_csv("difficulty_evaluation.csv")            # Read the dataset
difficulty_evaluation.drop_duplicates()                                     # Remove duplicate rows, if any

# Find and remove any outlier data entries
def find_outliers(dataColumn):
    outliers = []

    data_std = dataColumn.std()
    data_mean = dataColumn.mean()

    outlier_cutoff = data_std * 3
    lower_limit = data_mean - outlier_cutoff
    upper_limit = data_mean + outlier_cutoff

    for d in dataColumn:
        if d < lower_limit or d > upper_limit:
            outliers.append(d)

    return outliers

input_features = list(difficulty_evaluation.columns)
input_features.remove("Difficulty Evaluation")

for column in input_features:
  outliers = find_outliers(difficulty_evaluation[column])

  for d in outliers: difficulty_evaluation = difficulty_evaluation[difficulty_evaluation[column] != d]

# Scale floating-point values
scaler = MinMaxScaler()
scaled_features = ['Session Length', 'Percentage Robbers Tagged', 'Percentage Diamonds Collected']
scaled_columns = scaler.fit_transform(difficulty_evaluation.loc[:, scaled_features])
difficulty_evaluation[scaled_features] = scaled_columns

# Perform label encoding on string values
scale_mapper = {"Easier": 0, "Same": 1, "Harder": 2}
d = difficulty_evaluation['Difficulty Evaluation'].replace(scale_mapper)
difficulty_evaluation = difficulty_evaluation.drop(['Difficulty Evaluation'], axis=1)
d1 = pd.DataFrame(d)
d1.columns = ['Difficulty Evaluation']
difficulty_evaluation = pd.concat([difficulty_evaluation, d1], axis=1)

# Split the input features from the output column
X = difficulty_evaluation.values[:, 0:-1]
y = difficulty_evaluation.values[:, -1].astype('int')

# Split the dataset into training and testing sets
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=22)

# Train the XGBoostClassifier model on the training set and test its accuracy
xgBoost = xgb.XGBClassifier()
xgBoost.fit(X_train, y_train)

yp_training = xgBoost.predict(X_train)
accuracy_training = accuracy_score(yp_training, y_train)

yp_testing = xgBoost.predict(X_test)
accuracy_testing = accuracy_score(yp_testing, y_test)

print("Training Error:", round((1 - accuracy_training), 5), "\n")
print("Testing Error:", round((1 - accuracy_testing), 5), "\n")

# Store the model as a .pkl file
with open('difficulty_evaluation.pkl', 'wb') as model_file:
  pickle.dump(xgBoost, model_file)