# conda activate pandas

import pandas as pd 
import seaborn as sb   # pip install statsmodels
sb.set_theme()
import matplotlib.pyplot as plt
import numpy as np

# P1: player self
# P2: teamate
# P3: opponent 1
# P4: opponent 2
PATH = "c:/temp/"
COLUMNS_STATE_FILE = ["Date", "Seconds", "Role", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z", "Ballx", "Bally", "Ballz"]
COLUMNS_ACTION_FILE = ["Date", "Seconds", "Team", "Player", "Mov X", "Mov Z", "Target x", "Target z", "Shot", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z"]
def states(file, stat):
    df = pd.read_csv(file, delimiter=";", decimal=",", index_col=False, names=["Date", "Seconds", "Role", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z", "Ballx", "Bally", "Ballz"])
    print(df.describe())
    print(df)

    # remove first 80 rows
    df = df.iloc[80:]
    print(df)
    # reindex
    df = df.reset_index(drop=True)

    # remove one every 5 rows
    df = df[df.index % 5 == 0]
    df = df.reset_index(drop=True)


    print("FILTERED:")
    print(df)
    

    # convert to centimeters
    if False:
        df["P1x"] = df["P1x"]*100
        df["P1z"] = df["P1z"]*100
        df["P2x"] = df["P2x"]*100
        df["P2z"] = df["P2z"]*100
        df["P3x"] = df["P3x"]*100
        df["P3z"] = df["P3z"]*100
        df["P4x"] = df["P4x"]*100
        df["P4z"] = df["P4z"]*100
        df["Ballx"] = df["Ballx"]*100
        df["Ballz"] = df["Ballz"]*100

    # convert to integer
    '''
    df["P1x"] = df["P1x"].astype(int)
    df["P1z"] = df["P1z"].astype(int)
    df["P2x"] = df["P2x"].astype(int)
    df["P2z"] = df["P2z"].astype(int)
    df["P3x"] = df["P3x"].astype(int)
    df["P3z"] = df["P3z"].astype(int)
    df["P4x"] = df["P4x"].astype(int)
    df["P4z"] = df["P4z"].astype(int)
    df["Ballx"] = df["Ballx"].astype(int)
    df["Ballz"] = df["Ballz"].astype(int)
    '''
    
    print("CONVERTED:")
    print(df)
    print(df.columns)




    # Add row with time in seconds since start
    df["Seconds"] = (df["Seconds"] - df["Seconds"].iloc[0])/1000
    print(df)   
    
    # sort columns
    df = df[["Seconds", "Role", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z", "Ballx", "Ballz"]]  
    # rename columns
    df = df.rename(columns={"P1x": "Self x", "P1z": "Self y", "P2x": "Partner x", "P2z": "Partner y", "P3x": "Oppo1 x", "P3z": "Oppo1 y", "P4x": "Oppo2 x", "P4z": "Oppo2 y", "Ballx": "Ball x", "Ballz": "Ball y"})
    
    # get first n rows
    df = df.iloc[:15]
    
    # seconds column with 1 decimal
    df["Seconds"] = df["Seconds"].round(1)
    print(df)
    df = df.style.format(decimal='.', thousands='', precision=2)
    # hide index
    df = df.hide(axis="index")
    # fancy latex table
    df.to_latex("c:/temp/observations.tex")

    return

    df = df.iloc[:20000]
    if stat=="inter-distance":
        df["Distance between partners"] = np.sqrt((df["P1x"]-df["P2x"])*(df["P1x"]-df["P2x"]) + (df["P1z"]-df["P2z"])*(df["P1z"]-df["P2z"]))
        sb.displot(data=df, x="Distance between partners", kde=True)
        plt.show()

        df["Side-to-side distance between partners"] = np.abs((df["P1x"]-df["P2x"]))
        sb.displot(data=df, x="Side-to-side distance between partners", kde=True)
        plt.show()

        df["Net-to-Back distance between partners"] = np.abs((df["P1z"]-df["P2z"]))
        sb.displot(data=df, x="Net-to-Back distance between partners", kde=True)
        plt.show()

        return

    '''
    sb.displot(data=df, x="P1x", kde=True)
    plt.show()
    sb.displot(data=df, x="P1z", kde=True)
    plt.show()
    sb.jointplot(data=df, x="P1x", y="P1z", size=0.5, alpha=0.15)
    plt.show()
    '''
    #sb.lineplot(x="Seconds", y="P1z", data=df)
    #plt.show()

    '''
    sb.jointplot(data=df, x="P1x", y="P1z", kind="kde")
    sb.jointplot(data=df, x="P2x", y="P2z", kind="kde")
    sb.jointplot(data=df, x="P3x", y="P3z", kind="kde")
    sb.jointplot(data=df, x="P4x", y="P4z", kind="kde")
    plt.show()
    '''

    '''
    f, ax = plt.subplots(figsize=(6, 6))
    #sb.scatterplot(data=df, x="P1x", y="P1z", s=40, alpha=0.15)
    #sb.scatterplot(data=df, x="P3x", y="P3z", s=40, alpha=0.15)
    #sb.histplot(data=df, x="P1x", y="P1z", bins=20, pthresh=.1)
    sb.kdeplot(data=df, x="P1x", y="P1z", levels=8, color="b", linewidths=1)
    sb.kdeplot(data=df, x="P3x", y="P3z", levels=8, color="r", linewidths=1)
    plt.show()

    plt.cla()
    f, ax = plt.subplots(figsize=(6, 6))
    sb.kdeplot(data=df, x="P2x", y="P2z", levels=8, color="g", linewidths=1)
    sb.kdeplot(data=df, x="P4x", y="P4z", levels=8, color="m", linewidths=1)
    plt.show()
    '''

    


    df2 = df.melt(id_vars=["Date","Seconds", "P1z", "P2z", "P3z", "P4z", "Ballx", "Ballz"], var_name="Player", value_name="Posx")
    df2 = df2.melt(id_vars=["Date","Seconds", "Ballx", "Ballz", "Player", "Posx"], var_name="Player2", value_name="Posz")
    print(df2)
    
    g = sb.JointGrid(data=df2, x="Posx", y="Posz", space=0.5, ratio=2)
    g.plot_joint(sb.kdeplot, fill=True, clip=((-5, 5), (-10, 10)), thresh=0, levels=100, cmap="rocket")
    # Adjust the aspect ratio of the axes
    ax = g.ax_joint  # Get the joint axes
    ax.set_aspect("equal")  # Set the aspect ratio to 1:1 (equal scaling)

    plt.show()
    '''
    sb.kdeplot(data=df2, x="Posx", y="Posz", levels=8, hue="Player", linewidths=1)
    plt.show()
    '''

def action(file):
    df = pd.read_csv(file, delimiter=";", decimal=",", index_col=False, names=["Date", "Seconds", "Team", "Player", "Target x", "Target z", "Mov X", "Mov Z", "Ball x", "Ball z", "Shot"])
    print(df.describe())
    print(df)

    # rows[i] = $"{actions[i].time};{actions[i].seconds};{actions[i].team};{actions[i].player % 2};{actions[i].Xpos};{actions[i].Zpos};{actions[i].moveX};{actions[i].moveZ};{actions[i].xGrid};{actions[i].zGrid};{actions[i].hitType};";
            

    # remove first 80 rows
    df = df.iloc[4*80:]
    print(df)
    # reindex
    df = df.reset_index(drop=True)

    # get only rows for Team 1
    df = df[df["Team"]==1]
    # get only rows for Player 1
    df = df[df["Player"]==0]    
    df = df.reset_index(drop=True)

    # remove one every 5 rows
    df = df[df.index % 25 == 0] #
    df = df.reset_index(drop=True)
    print(df)

     # Add row with time in seconds since start
    df["Seconds"] = (df["Seconds"] - df["Seconds"].iloc[0])/1000
    print(df)   
    
    # sort columns
    #df = df[["Seconds", "Team", "Player", "Mov X", "Mov Z", "Target x", "Target z", "Shot", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z"]]
    #df = df[["Seconds", "Mov X", "Mov Z", "Target x", "Target z", "Shot", "Ball x", "Ball z"]]
    df = df[["Seconds", "Mov X", "Mov Z", "Shot", "Ball x", "Ball z"]]
    df["Mov X"] = df["Mov X"]*0.02 # fixed delta time
    df["Mov Z"] = df["Mov Z"]*0.02 # fixed delta time
    # rename columns
    df = df.rename(columns={"Mov X": "Move x", "Mov Z": "Move y", "Ball x": "Ball target x", "Ball z": "Ball target y"})
    print(df)

    

    # get first n rows
    df = df.iloc[:15]  # 15
    
    # seconds column with 1 decimal
    df["Seconds"] = df["Seconds"].round(1)
    print(df)
    df = df.style.format(decimal='.', thousands='', precision=2)
    # hide index
    df = df.hide(axis="index")
    # fancy latex table
    df.to_latex("c:/temp/actions.tex")


    return
    df = df.iloc[:20000]

    def classify_distance(value):
        if abs(value) < 10/3:
            return "cerca"
        elif abs(value) > 2*10/3:
            return "lejos"
        else:
            return "medio"
    
    df = df[df['Shot'] != 0]
    # Define el diccionario de mapeo de valores
    mapping = {1: "normal", 2: "cortado", 3: "topspin", 4: "globo",5: "remate"}

    # Reemplaza los valores en la columna "Shot" utilizando el diccionario
    df['Shot'] = df['Shot'].replace(mapping)
    df['opponent 1 position'] = df['P3z'].apply(classify_distance)
    df['opponent 2 position'] = df['P4z'].apply(classify_distance)
    print(df)
    sb.countplot(data=df, x="Shot",hue="opponent 1 position")
    plt.show()
    sb.countplot(data=df, x="Shot",hue="opponent 2 position")
    plt.show()


    g = sb.JointGrid(data=df, x="Target x", y="Target z", space=0.5, ratio=2)
    g.plot_joint(sb.kdeplot, fill=True, clip=((-5, 5), (-10, 10)), thresh=0, levels=100, cmap="rocket")
    ax = g.ax_joint  # Get the joint axes
    ax.set_aspect("equal")  # Set the aspect ratio to 1:1 (equal scaling)
    plt.show()



def compare_models(models, variable):
    print("Reading data...")
    dfs = []
    for item in models:
        model_suffix = item[0]
        value = item[1]
        df = pd.read_csv(f"{PATH}stateLog{model_suffix}.csv", delimiter=";", decimal=",", index_col=False, names=COLUMNS_STATE_FILE)
        df = df.iloc[:20000]
        df[variable] = value
        print(df.describe())
        print(df)
        dfs.append(df)

    print("Appending...")
    dfall = dfs[0].copy()
    for d in dfs[1:]:
        dfall = dfall._append(d, ignore_index=True)
        print(df.describe())
        print(df)

    # heat maps
    if False:
        for i, df in enumerate(dfs):
            df = df.copy()
            df2 = df.melt(id_vars=["Date","Seconds", "P1z", "P2z", "P3z", "P4z", "Ballx", "Ballz", variable], var_name="Player", value_name="Pos x")
            df2 = df2.melt(id_vars=["Date","Seconds", "Ballx", "Ballz", "Player", "Pos x", variable], var_name="Player2", value_name="Pos z")
            print(df2)
            g = sb.JointGrid(data=df2, x="Pos x", y="Pos z", space=0) #, title=f"{variable}={models[i][1]}")
            g.plot_joint(sb.kdeplot, fill=True, clip=((-5, 5), (-10, 10)), thresh=0, levels=100, cmap="rocket")
            plt.show()

    # distances 
    df = dfall
    if False:
        print(df)
        var = "2D Distance between partners"
        df[var] = np.sqrt((df["P1x"]-df["P2x"])*(df["P1x"]-df["P2x"]) + (df["P1z"]-df["P2z"])*(df["P1z"]-df["P2z"]))
        sb.kdeplot(data=df, x=var, hue=variable, clip=(0,10))
        plt.show()

        var = "Side-to-side distance between partners"
        df[var] = np.abs((df["P1x"]-df["P2x"]))
        sb.kdeplot(data=df, x=var, hue = variable, clip=(0,10))
        plt.show()

        var = "Net-Back distance between partners"
        df[var] = np.abs((df["P1z"]-df["P2z"]))
        sb.kdeplot(data=df, x=var, hue = variable, clip=(0,10))
        plt.show()

    # correlation among players
    df = dfall.copy()
    #df["P1x"]+=np.random.normal(0, 0.5, df.shape[0]) 
    #df["P2x"]+=np.random.normal(0, 0.5, df.shape[0]) 
    #sb.lmplot(data=df, x="P1x", y="P2x", hue=variable, col=variable, robust=True, x_jitter=0.2, y_jitter=0.2, scatter_kws={"alpha":0.25})
    #plt.show()

    sb.lmplot(data=df, x="P1x", y="Ballx", hue=variable, col=variable, robust=True, x_jitter=0.2, y_jitter=0.2, scatter_kws={"alpha":0.25})
    plt.show()


def compare_model_actions(models, variable):
    print("Reading data...")
    dfs = []
    for item in models:
        model_suffix = item[0]
        value = item[1]
        df = pd.read_csv(f"{PATH}actionLog{model_suffix}.csv", delimiter=";", decimal=",", index_col=False, names=COLUMNS_ACTION_FILE)
        df = df.iloc[:20000]
        df[variable] = value
        print(df.describe())
        print(df)
        dfs.append(df)
    
    print("Appending...")
    dfall = dfs[0].copy()
    for d in dfs[1:]:
        dfall = dfall._append(d, ignore_index=True)
        print(df.describe())
        print(df)

    # heat map 
    for i, df in enumerate(dfs):
        df = df.copy()
        g = sb.JointGrid(data=df, x="Target x", y="Target z", space=0) 
        g.plot_joint(sb.kdeplot, fill=True, clip=((-1, 5), (-1, 5)), thresh=0, levels=100, cmap="rocket")
        plt.show()
    

    # shots types  TODO: discount influence of episode length
    df = dfall[dfall['Shot']!=0]
    # Define el diccionario de mapeo de valores
    mapping = {1: "normal", 2: "cortado", 3: "topspin", 4: "globo",5: "remate"}

    # Reemplaza los valores en la columna "Shot" utilizando el diccionario
    df['Shot'] = df['Shot'].replace(mapping)
    print(df)
    sb.countplot(data=df, x="Shot", hue = variable)
    plt.show()



  

#models = [("92", "3 m/s"), ("91", "4 m/s"), ("93", "6 m/s")] #, ("-untrained", "learners")  # (model, value)
#compare_models(models, "Max speed")
#compare_model_actions(models, "Max speed")
#states("c:/temp/stateLog-RL-rand.csv", "x")
action("c:/temp/actionLog-RL-rand.csv")
