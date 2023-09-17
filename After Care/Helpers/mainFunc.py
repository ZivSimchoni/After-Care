from selenium.webdriver.common.by import By
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
import time
import requests
import os
import json
import urllib
import sys

APP_DEFAULT_FOLDER_TO_DOWNLOAD = "/AfterCareApks/"


def searchApp(jsonFileLocation, appName):
    with open(jsonFileLocation + "/../appsLinkDict.json", "r") as f:
        appsLinkDict = json.load(f)
    for category, apps in appsLinkDict.items():
        for app, details in apps.items():
            if app == appName:
                return {
                    "Category": category,
                    "App": app,
                    "URL": details["url"],
                    "Icon": details["icon"],
                    "Description": details["Description"],
                }


def mainScrape(jsonFileLocation, listOfApps, beta):
    for App in listOfApps:
        # downloadWebSite,downloadLink = searchForApp(App)
        download = searchApp(jsonFileLocation, App)["URL"]
        # downloadListTemp = download.split('/')
        # downloadparse = urlparse(download)
        # downloadWebSite =  downloadparse.hostname
        if download.startswith("https://www.apkmirror.com/"):
            downloadAPKMirror(download, beta)
        elif download.startswith("https://github.com/"):
            downloadGitHub(download)
        elif download.startswith("https://f-droid.org/"):
            downloadFDroid(download)
        elif download.startswith("https://apt.izzysoft.de/"):
            downloadIzzySoft(download)
        elif download.startswith("https://thedise.me/"):
            downloadTheDise(download)
        elif download.startswith("https://telegram.org/"):
            downloadTelegram(download)
        elif download.startswith("https://proton.me/"):
            downloadProtonMail(download)
        elif download.startswith("https://store.steampowered.com/"):
            downloadSteam(download)
        elif download.startswith("https://mixplorer.com/"):
            downloadMixPlorer(download)
        else:  # not supported
            return


def initSelenium():
    downloads_folder = os.path.join(os.environ["USERPROFILE"], "Downloads")
    #################Init######################
    local_state = {
        "dns_over_https.mode": "secure",
        "dns_over_https.templates": "https://dns.adguard-dns.com/dns-query",  # To avoid Ads
    }
    options = Options()
    options.add_argument("--headless=new")
    os.getcwd()

    prefs = {
        "profile.default_content_settings.popups": 0,
        "download.default_directory": downloads_folder,  # Set the path accordingly
        "download.prompt_for_download": False,  # change the download path accordingly
        "download.directory_upgrade": True,
    }
    options.add_experimental_option("prefs", prefs)
    options.add_experimental_option("localState", local_state)
    options.add_argument("--disable-usb-keyboard-detect")
    driver = webdriver.Chrome(options=options)
    return driver


def downloadAPKMirror(downloadLink, beta):
    driver = initSelenium()
    driver.get(downloadLink)
    seleniumUserAgent = driver.execute_script("return navigator.userAgent")
    time.sleep(1)
    driver.execute_script(
        "arguments[0].scrollIntoView();",
        driver.find_element(By.XPATH, "/html/body/div[2]/div/div[1]/div[5]"),
    )
    time.sleep(3)
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

    divOfAllVersions = driver.find_element(
        By.XPATH, "/html/body/div[2]/div/div[1]/div[5]"
    )
    try:
        splitfTable = divOfAllVersions.find_element(By.CLASS_NAME, "appRow")
    except:
        divOfAllVersions = driver.find_element(
            By.XPATH, "/html/body/div[2]/div/div[1]/div[6]"
        )
        splitfTable = divOfAllVersions.find_element(By.CLASS_NAME, "appRow")
    time.sleep(1)
    splitfTable.find_element(By.CLASS_NAME, "downloadIconPositioning").click()

    fileNameVersion = driver.find_element(By.TAG_NAME, "h1").text

    if not isFileNeedsToBeDownloaded(fileNameVersion):
        driver.quit()
        return

    driver.execute_script(f"window.scrollTo(0, 800);")
    driver.find_element(
        By.XPATH, '//*[@id="downloads"]/div/div/div[2]/div[5]/a'
    ).click()
    driver.execute_script(f"window.scrollTo(0, 300);")
    try:
        driver.find_element(By.XPATH, '//*[@id="file"]/div[1]/div[2]/div/a').click()
    except:
        driver.find_element(
            By.XPATH,
            "/html/body/div[2]/div/div[1]/article/div[2]/div[3]/div[1]/div[2]/div[2]/div/a",
        ).click()

    downloadlink = driver.find_element(
        By.XPATH,
        "/html/body/div[2]/div/div[1]/article/div[2]/div/div/div[1]/p[2]/span/a",
    ).get_attribute("href")
    cookies = driver.get_cookies()
    session = requests.Session()
    headers = {"User-Agent": seleniumUserAgent}
    session.headers.update(headers)
    for cookie in cookies:
        session.cookies.set(cookie["name"], cookie["value"])
    response = session.get(downloadlink)
    downloads_folder = os.path.join(os.environ["USERPROFILE"], "Downloads")
    latest_download = max(
        os.listdir(downloads_folder),
        key=lambda x: os.path.getctime(os.path.join(downloads_folder, x)),
    )
    saveFile(fileNameVersion + ".apk", response, driver)
    os.remove(os.path.join(downloads_folder, latest_download))
    # Close the WebDriver session when done


def downloadGitHub(downloadLink):
    driver = initSelenium()
    # driver.manage().timeouts().implicitlyWait()
    driver.get(downloadLink)
    driver.execute_script(f"window.scrollTo(0, 800);")
    time.sleep(1)
    downloadHREF = driver.find_element(By.PARTIAL_LINK_TEXT, "apk").get_attribute(
        "href"
    )
    fileNameVersion = downloadHREF.split("/")[-1]
    if isFileNeedsToBeDownloaded(fileNameVersion):
        session = requests.Session()
        response = session.get(downloadHREF)
        saveFile(fileNameVersion, response, driver)


def downloadFDroid(downloadLink):
    driver = initSelenium()
    if downloadLink == "https://f-droid.org/F-Droid.apk":
        appName = "F"
        appVersion = "-Droid"
        downloadHref = downloadLink
    else:
        driver.get(downloadLink)
        downloadHref = (
            driver.find_element(By.CLASS_NAME, "package-version-download")
            .find_element(By.TAG_NAME, "a")
            .get_attribute("href")
        )
        appName = driver.find_element(By.CLASS_NAME, "package-name").text
        appVersion = (
            driver.find_element(By.CLASS_NAME, "package-version-header")
            .find_element(By.TAG_NAME, "a")
            .get_attribute("name")
        )
    fileNameVersion = appName + appVersion + ".apk"
    if isFileNeedsToBeDownloaded(fileNameVersion):
        session = requests.Session()
        response = session.get(downloadHref)
        saveFile(fileNameVersion, response, driver)


def downloadIzzySoft(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    fileNameVersion = driver.find_element(By.TAG_NAME, "h2").text
    fileNameVersion += (
        "_"
        + driver.find_element(
            By.XPATH, "/html/body/div[1]/div[2]/table/tbody/tr[8]/td[2]"
        ).text
    )
    fileNameVersion += ".apk"
    if isFileNeedsToBeDownloaded(fileNameVersion):
        downloadlink = driver.find_element(
            By.XPATH, "/html/body/div[1]/div[4]/center/a[1]"
        ).get_attribute("href")
        session = requests.Session()
        response = session.get(downloadlink)
        saveFile(fileNameVersion, response, driver)


def downloadSteam(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    driver.execute_script(
        "arguments[0].scrollIntoView();",
        driver.find_element(By.CLASS_NAME, "mobile_footer_text"),
    )
    downloadlink = driver.find_element(
        By.XPATH,
        "/html/body/div[1]/div[7]/div[6]/div[1]/div[2]/div[3]/div/div[3]/div[2]/a",
    ).get_attribute("href")
    fileNameVersion = downloadlink.split("/")[-1]
    if isFileNeedsToBeDownloaded(fileNameVersion):
        session = requests.Session()
        response = session.get(downloadlink)
        saveFile(fileNameVersion, response, driver)


def downloadMixPlorer(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    fileNameVersion = driver.find_element(By.CLASS_NAME, "file").text
    if isFileNeedsToBeDownloaded(fileNameVersion):
        downloadlink = driver.find_element(By.CLASS_NAME, "apk").get_attribute("href")
        session = requests.Session()
        response = session.get(downloadlink)
        saveFile(fileNameVersion, response, driver)


def downloadTelegram(downloadLink):
    # TODO: make this better # note that driver isn't used
    driver = initSelenium()
    fileNameVersion = "Telegram.apk"
    if isFileNeedsToBeDownloaded(fileNameVersion):
        session = requests.Session()
        response = session.get(downloadLink)
        saveFile(fileNameVersion, response, driver)


def downloadProtonMail(downloadLink):
    # TODO: make this better # note that driver isn't used
    driver = initSelenium()
    fileNameVersion = "ProtonMail-Android.apk"
    if isFileNeedsToBeDownloaded(fileNameVersion):
        session = requests.Session()
        response = session.get(downloadLink)
        saveFile(fileNameVersion, response, driver)


def downloadTheDise(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    fileNameVersion = driver.find_element(
        By.XPATH, "/html/body/div[2]/main/div[2]/div[1]/div/table/tbody/tr[1]/th[1]/a"
    ).text
    fileNameVersion += ".apk"
    if isFileNeedsToBeDownloaded(fileNameVersion):
        downloadlink = driver.find_element(
            By.XPATH,
            "/html/body/div[2]/main/div[2]/div[1]/div/table/tbody/tr[1]/th[1]/a",
        ).get_attribute("href")
        session = requests.Session()
        response = session.get(downloadlink)
        saveFile(fileNameVersion, response, driver)


def isFileNeedsToBeDownloaded(fileNameToCheck):
    downloads_folder = (
        os.path.join(os.environ["USERPROFILE"], "Downloads")
        + APP_DEFAULT_FOLDER_TO_DOWNLOAD
    )
    # No such folder or no such file - Download it
    if (not (os.path.exists(downloads_folder))) or (
        not (os.path.isfile(downloads_folder + fileNameToCheck))
    ):
        return True
    # print(f"No need to download {fileNameToCheck} - Skip!")
    return False  # Else: file found - Do not download


def saveFile(fileNameVersion, response, driver):
    # check if response is successful and only then continue
    if fileNameVersion == "app-release.apk":
        fileNameVersion = (
            driver.find_element(
                By.XPATH,
                """//*[@id="repo-content-pjax-container"]/div/div/div/div[1]/div[1]/div[1]/div[1]/h1""",
            ).text
            + ".apk"
        )
    downloads_folder = (
        os.path.join(os.environ["USERPROFILE"], "Downloads")
        + APP_DEFAULT_FOLDER_TO_DOWNLOAD
    )
    if not (os.path.exists(downloads_folder)):
        os.makedirs(downloads_folder + APP_DEFAULT_FOLDER_TO_DOWNLOAD[-1])
    local_file_path = downloads_folder + fileNameVersion

    with open(local_file_path, "wb") as file:
        file.write(response.content)
    driver.quit()

    print("successfully downloaded " + fileNameVersion)


# def searchForApp(nameOfApp):
#     for website in data['websites']:
#         try:
#             if website['apps'][nameOfApp]:
#                 return website['url'],website['apps'][nameOfApp]
#         except:
#             continue


# def testing():
#     listOfApps = []
#     for category, apps in appsLinkDict.items():
#         for app, details in apps.items():
#             listOfApps.append(app)
#     beta = True
# mainScrape(listOfApps[4:],beta)

mainScrape(sys.argv[1], sys.argv[2:-1], sys.argv[-1])
# mainScrape(
#     r"C:\Users\Ziv S\source\repos\AfterCare\After Care\bin\x64\Debug\net7.0-windows10.0.19041.0\win10-x64\AppX\Helpers\mainFunc.py",
#     [
#         "Firefox",
#     ],
#     True,
# )
