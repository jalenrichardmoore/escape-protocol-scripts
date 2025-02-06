# **Escape Protocol - Scripts**

## **Overview**
This repository contains all necessary scripts for the Unity game, *Escape Protocol*. *Escape Protocol* is a casual game that simulates "Cops & Robbers," integrating an XGBoost Classifier machine learning algorithm to evaluate player performance each round and determine how to adjust the game's difficulty through changing the enemy agents' AI. These scripts control player mechanics, define the enemy AI behavior, and manage the flow of each level. Included also are the Python scripts necessary to create the training dataset and algorithm model used for difficulty evaluation.

## **Folder Structure**
```plaintext
/Model Training Scripts/
│   ├── create_dataset.py                 # Creates a dataset used to train the evaluation model
│   ├── create_model.py                   # Trains an XGBoostClassifier model on the dataset
│   ├── difficulty_evaluation.csv         # The training dataset
│   ├── difficulty_evaluation.pkl         # The evaluation model
│   ├── difficulty_evaluation.py          # Calls the evaluation model and adjusts the game difficulty

/Scripts/
|-- Agents/
│   ├── CopController.cs                  # Defines the AI behavior for the cop agent
│   ├── RobberController.cs               # Defines the AI behavior for the robber agent
|-- Audio/
│   ├── AudioManager.cs                   # Controls the audio for the entire game
|-- Downtown/
│   ├── DepositZone.cs                    # Defines the properties of the deposit zone game object
│   ├── Diamond.cs                        # Defines the properties of the diamond game object
│   ├── DowntownManager.cs                # Manages the game flow during the 'Downtown' level
│   ├── SpawnPoint.cs                     # Defines the properties of the spawn point game object
|-- Evaluation Screen/
│   ├── EvaluationManager.cs              # Manages the game flow of the 'Evaluation Screen' level
|-- Gameplay/
│   ├── Cop.cs                            # Defines the properties and abilities of the cop game object
│   ├── PlayerMovement.cs                 # Handles player controls
│   ├── Robber.cs                         # Defines the properties and abilities of the robber game object
|-- Management/
│   ├── GameData.cs                       # Stores global data used to manage game difficulty
|-- Title Screen/
│   ├── RoleInfo.cs                       # Displays the player's goal for the selected character role
│   ├── TitleManager.cs                   # Manages UI screens during the 'Title Screen' level
└── README.md
```

## **Dependencies**
Unity Version: 2022.3.3

Python Version: 3.9.13 

Python libraries: numpy, pandas, pickle, xgboost

## **Installation & Usage**
1. Clone the repository:
```sh
git clone https://github.com/Jalen-Moore/escape-protocol-scripts.git
```

2. Copy the scripts in the 'Scripts' into your Unity project's 'Assets/Scripts/' folder

3. Copy the 'difficulty_evaluation.pkl' and 'difficulty_evaluation.py' files into your Unity projects 'Asseets/StreamingAssets/' folder

4. Add your Python version's .ddl file into your Unity projects 'Asseets/StreamingAssets/' folder

## **Credits**
Developed by Jalen Moore
