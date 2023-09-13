from selenium.webdriver.common.by import By
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
import time
import requests
import os
import json
import urllib
import sys


def searchApp(appName):
    with open(r'C:\Users\Ziv S\source\repos\AfterCare\After Care\Helpers\appsLinkDict.json', 'r') as f:
        appsLinkDict = json.load(f)
    for category, apps in appsLinkDict.items():
        for app,details in apps.items():
            if app == appName:
                return {
                    "Category": category,
                    "App": app,
                    "URL":details["url"],
                    "Icon": details["icon"],
                    "Description": details["Description"]
                }


def mainScrape(listOfApps,beta):
    for App in listOfApps:
        #downloadWebSite,downloadLink = searchForApp(App)
        download = searchApp(App)["URL"]
        #downloadListTemp = download.split('/')
        #downloadparse = urlparse(download)
        #downloadWebSite =  downloadparse.hostname
        if download.startswith('https://www.apkmirror.com/'):
            downloadAPKMirror(download,beta)
        elif download.startswith('https://github.com/'):
            downloadGitHub(download)
        elif download.startswith('https://f-droid.org'):
            downloadFDroid(download)


def initSelenium():
    downloads_folder = os.path.join(os.environ['USERPROFILE'], 'Downloads')
    #################Init######################
    local_state = {
        "dns_over_https.mode": "secure",
        "dns_over_https.templates": "https://dns.adguard-dns.com/dns-query",
    }
    options = Options()
    options.add_argument("--headless=new")
    os.getcwd()
    #download_dir = os.getcwd() + r'\tempks'


    prefs = {"profile.default_content_settings.popups": 0,
             "download.default_directory": downloads_folder,  ### Set the path accordingly
             "download.prompt_for_download": False,  ## change the downpath accordingly
             "download.directory_upgrade": True,}
    options.add_experimental_option("prefs", prefs)

    options.add_experimental_option('localState', local_state)
    options.add_argument("--disable-usb-keyboard-detect")
    driver = webdriver.Chrome(options=options)
    return driver

def downloadGitHub(downloadLink):
    driver = initSelenium()
    #driver.manage().timeouts().implicitlyWait()
    driver.get(downloadLink)
    driver.execute_script(f"window.scrollTo(0, 800);")
    time.sleep(1)
    downloadHREF = driver.find_element(By.PARTIAL_LINK_TEXT,"apk").get_attribute("href")
    fileNameVersion = downloadHREF.split('/')[-1]
    session = requests.Session()
    response = session.get(downloadHREF)
    saveFile(fileNameVersion,response,driver)

def downloadAPKMirror(downloadLink,beta):

    driver = initSelenium()

    driver.get(downloadLink)
    seleniumUserAgent = driver.execute_script("return navigator.userAgent")

    time.sleep(1)

    driver.execute_script("arguments[0].scrollIntoView();",driver.find_element(By.XPATH,"/html/body/div[2]/div/div[1]/div[5]"))

    time.sleep(2)
    # try:
    #     listOfAvailVersions = driver.find_element(By.XPATH,"/html/body/div[2]/div/div[1]/div[5]").find_elements(By.TAG_NAME,'h5')
    # except:
    #     driver.refresh()
    #     time.sleep(1)
    #     listOfAvailVersions = driver.find_element(By.XPATH,"/html/body/div[2]/div/div[1]/div[5]").find_elements(By.TAG_NAME,'h5')
    #     if len(listOfAvailVersions) == 0:
    #         print("unable to find list of version, refreshing and trying again")
    #         return downloadAPKMirror(downloadLink,beta)
    # if (beta):
    #     listOfAvailVersions[0].click()
    # else:
    #     for div_element in listOfAvailVersions:
    #         text_content = div_element.text
    #         if "beta" not in text_content:
    #             div_element.click()
    #             break
    listOfAvailableVersions = driver.find_element(By.XPATH, """//*[@id="primary"]/div[6]""").find_elements(By.TAG_NAME, "h5")
    listOfAvailableVersions[0].click()

    fileNameVersion = driver.find_element(By.TAG_NAME,'h1').text

    if(os.path.exists(os.path.join('apks',fileNameVersion))):
        print(fileNameVersion + "already downloaded skipping...")

    driver.execute_script(f"window.scrollTo(0, 800);")
    driver.find_element(By.XPATH, '//*[@id="downloads"]/div/div/div[2]/div[5]/a').click()
    driver.execute_script(f"window.scrollTo(0, 300);")
    try:
        driver.find_element(By.XPATH, '//*[@id="file"]/div[1]/div[2]/div/a').click()
    except:
        driver.find_element(By.XPATH,'/html/body/div[2]/div/div[1]/article/div[2]/div[3]/div[1]/div[2]/div[2]/div/a').click()


    downloadlink = driver.find_element(By.XPATH,
                                       '/html/body/div[2]/div/div[1]/article/div[2]/div/div/div[1]/p[2]/span/a').get_attribute(
        "href")
    cookies = driver.get_cookies()
    session = requests.Session()
    headers = {
        'User-Agent': seleniumUserAgent
    }
    session.headers.update(headers)
    for cookie in cookies:
        session.cookies.set(cookie['name'], cookie['value'])
    response = session.get(downloadlink)
    downloads_folder = os.path.join(os.environ['USERPROFILE'], 'Downloads')
    latest_download = max(os.listdir(downloads_folder), key=lambda x: os.path.getctime(os.path.join(downloads_folder, x)))
    saveFile(fileNameVersion + ".apk",response,driver)
    os.remove(os.path.join(downloads_folder, latest_download))
    # Close the WebDriver session when done


def saveFile(fileNameVersion,response,driver):
    #check if response is succeful and only then continue
    if fileNameVersion == "app-release.apk":
        fileNameVersion = driver.find_element(By.XPATH,"""//*[@id="repo-content-pjax-container"]/div/div/div/div[1]/div[1]/div[1]/div[1]/h1""").text + ".apk"
    downloads_folder = os.path.join(os.environ['USERPROFILE'], 'Downloads')
    if not(os.path.exists(downloads_folder+"/AfterCareApks")):
        os.makedirs(downloads_folder+"/AfterCareApks")
    local_file_path = downloads_folder + "/AfterCareApks/" + fileNameVersion

    with open(local_file_path, 'wb') as file:
        file.write(response.content)
    driver.quit()

    print('successfully downloaded ' + fileNameVersion)

# def searchForApp(nameOfApp):
#     for website in data['websites']:
#         try:
#             if website['apps'][nameOfApp]:
#                 return website['url'],website['apps'][nameOfApp]
#         except:
#             continue


def downloadFDroid(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    if downloadLink == "https://f-droid.org/":
        downloadHref = driver.find_element(By.ID, "fdroid-download").get_attribute("href")
        appName = "Fdroid"
        appVersion = "0.0"
    else:
        downloadHref = driver.find_element(By.CLASS_NAME,"package-version-download").find_element(By.TAG_NAME,"a").get_attribute("href")
        appName = driver.find_element(By.CLASS_NAME,"package-name").text
        appVersion = driver.find_element(By.CLASS_NAME,"package-version-header").find_element(By.TAG_NAME, "a").get_attribute("name")
    session = requests.Session()
    response = session.get(downloadHref)
    saveFile(appName + appVersion,response,driver)


# def testing():
#     listOfApps = []
#     for category, apps in appsLinkDict.items():
#         for app, details in apps.items():
#             listOfApps.append(app)
#     beta = True
    # mainScrape(listOfApps[4:],beta)


import sys
print(sys.argv[1:-1])
print(sys.argv[-1])
mainScrape(sys.argv[1:-1],sys.argv[-1])
