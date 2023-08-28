from mainFunc import initSelenium,data
import urllib,json
from selenium.webdriver.common.by import By

def getIcons():

    driver = initSelenium()
    for cat in data:
        for app in data[cat]:
            if data[cat][app]['url'].startswith("https://www.apkmirror"):
                driver.get(data[cat][app]['url'])
                url = driver.find_element(By.XPATH,"/html/body/div[1]/div/header/div/div/div[1]/img").get_attribute("src")
                urllib.request.urlretrieve(url,"icons/"  + app + ".png")
                data[cat][app]["icon"] = url
                print("downloaded "+ app)
            if data[cat][app]['url'].startswith("https://f"):
                driver.get(data[cat][app]['url'])
                url = driver.find_element(By.XPATH, "/html/body/div/div/div[1]/article/header/img").get_attribute("src")
                urllib.request.urlretrieve(url,"icons/"  + app + ".png")
                data[cat][app]["icon"] = url
                print("downloaded "+ app)
    #iconjson = json.dumps(dic)
    with open('data.json', 'w') as json_file:
        # Write the JSON data to the file
        json.dump(data, json_file)
