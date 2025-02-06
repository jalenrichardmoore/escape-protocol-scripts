# Import libraries
import numpy as np
import pandas as pd

num_entries = 10000                                                         # Number of entries

# Generate 'Success State,' 'Player Type,' & 'Session Time' Columns with random data
success_state = np.random.randint(0, 2, num_entries)
player_type = np.random.randint(0, 2, num_entries)
session_time = np.random.uniform(0, 180, num_entries)

# Initialize 'Percentage Robbers Tagged,' 'Times Sped Up,' 'Percentage Diamonds Collected,' and 'Times Hidden' with default values
percentage_robbers_tagged = np.zeros(num_entries)
times_sped_up = np.zeros(num_entries, dtype=int)
percentage_diamonds_collected = np.zeros(num_entries)
times_hidden = np.zeros(num_entries, dtype=int)

# Apply rules for 'Percentage Robbers Tagged'
for i in range(num_entries):
  if success_state[i] == 1 and player_type[i] == 0: percentage_robbers_tagged[i] = 100
  elif success_state[i] == 0 and player_type[i] == 1: percentage_robbers_tagged[i] = 100
  elif success_state[i] == 1 and player_type[i] == 1: percentage_robbers_tagged[i] = 0
  else: percentage_robbers_tagged[i] = np.random.randint(0, 100)

# Apply rules for 'Times Sped Up'
times_sped_up[player_type == 0] = np.random.randint(0, 16, np.sum(player_type == 0))

# Apply rules for 'Percentage Diamonds Collected'
for i in range(num_entries):
  if success_state[i] == 1 and player_type[i] == 1: percentage_diamonds_collected[i] = 100
  elif success_state[i] == 0 and player_type[i] == 0: percentage_diamonds_collected[i] = 100
  else: percentage_diamonds_collected[i] = np.random.randint(0, 100)

# Apply rules for 'Times Hidden'
times_hidden[player_type == 1] = np.random.randint(0, 16, np.sum(player_type == 1))

# Create dataset
data = {
    'Success State': success_state,
    'Session Length': session_time,
    'Player Type': player_type,
    'Percentage Robbers Tagged': percentage_robbers_tagged,
    'Times Sped Up': times_sped_up,
    'Percentage Diamonds Collected': percentage_diamonds_collected,
    'Times Hidden': times_hidden,
    'Difficulty Evaluation': [''] * num_entries
}

difficulty_dataset = pd.DataFrame(data)

# Generate output value for each data entry based on input features
def evaluate_entry(entry):
  score = 0

  if entry['Success State'] == 0: score += 1
  elif entry['Success State'] == 1: score += -1

  if entry['Session Length'] >= 90: score += 1
  elif entry['Session Length'] <= 60: score += -1

  if entry['Player Type'] == 0:
    if entry['Percentage Robbers Tagged'] <= 40: score += 1
    elif entry['Percentage Robbers Tagged'] >= 60: score += -1

    if entry['Times Sped Up'] >= 10: score += 1
    elif entry['Times Sped Up'] <= 5: score += -1

    if entry['Percentage Diamonds Collected'] >= 60: score += 1
    elif entry['Percentage Diamonds Collected'] <= 40: score += -1

    if entry['Times Hidden'] <= 5: score += 1
    elif entry['Times Hidden'] >= 10: score += -1
  elif entry['Player Type'] == 1:
    if entry['Percentage Diamonds Collected'] <= 40: score += 1
    elif entry['Percentage Diamonds Collected'] >= 60: score += -1

    if entry['Times Hidden'] >= 10: score += 1
    elif entry['Times Hidden'] <= 5: score += -1

    if entry['Percentage Robbers Tagged'] >= 60: score += 1
    elif entry['Percentage Robbers Tagged'] <= 40: score += -1

    if entry['Times Sped Up'] <= 5: score += 1
    elif entry['Times Sped Up'] >= 10: score += -1

  if score > 0: return "Easier"
  elif score < 0: return "Harder"
  else: return "Same"

difficulty_dataset["Difficulty Evaluation"] = difficulty_dataset.apply(evaluate_entry, axis=1)

difficulty_dataset.to_csv("difficulty_evaluation.csv", index=False)         # Store the dataset as a .csv file