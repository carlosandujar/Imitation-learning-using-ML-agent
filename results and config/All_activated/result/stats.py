# conda activate pandas

import pandas as pd 
import seaborn as sb   # pip install statsmodels
sb.set_theme()
import matplotlib.pyplot as plt
import numpy as np

# P1: player self
# P2: teamate
# P1: opponent 1
# P1: opponent 2
PATH = "c:/temp/"
COLUMNS_STATE_FILE = ["Date", "Seconds", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z", "Ballx", "Ballz"]
COLUMNS_ACTION_FILE = ["Date", "Seconds", "Team", "Player", "Mov X", "Mov Z", "Target x", "Target z", "Shot", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z"]
def states(file, stat):
    df = pd.read_csv(file, delimiter=";", decimal=".", index_col=False, names=["Date", "Seconds", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z", "Ballx", "Ballz"])
    print(df.describe())
    print(df)

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
    
    g = sb.JointGrid(data=df2, x="Posx", y="Posz", space=0, ratio=2)
    g.plot_joint(sb.kdeplot, fill=True, clip=((-5, 5), (-10, 10)), thresh=0, levels=100, cmap="rocket")
    plt.show()
    '''
    sb.kdeplot(data=df2, x="Posx", y="Posz", levels=8, hue="Player", linewidths=1)
    plt.show()
    '''

def action(file):
    df = pd.read_csv(file, delimiter=";", decimal=".", index_col=False, names=["Date", "Seconds", "Team", "Player", "Mov X", "Mov Z", "Target x", "Target z", "Shot", "P1x", "P1z", "P2x", "P2z", "P3x", "P3z", "P4x", "P4z"])
    print(df.describe())
    print(df)

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


    g = sb.JointGrid(data=df, x="Target x", y="Target z", space=0)
    g.plot_joint(sb.kdeplot, fill=True, clip=((-5, 5), (-10, 10)), thresh=0, levels=100, cmap="rocket")
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
states("c:/temp/stateLog.csv", "x")
action("c:/temp/actionLog.csv")
