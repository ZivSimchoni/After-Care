from selenium.webdriver.common.by import By
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
import time
import requests
import os
import json


#jsonFile = json.load("appsLinkDict.json")
with open('appsLinkDict.json', 'r') as f:
    data = json.load(f)
download_dir = os.getcwd() + r'\tempks'
#websiteScrape = 'https://www.apkmirror.com/'
# appLinkDict = {'waze':'apk/waze/waze-gps-maps-traffic-alerts-live-navigation/',
#                'chrome':'apk/google-inc/chrome/',
#                'firefox':'apk/mozilla/firefox/',
#                'brave':'apk/brave-software/brave-browser/',
#                'kiwi':'apk/geometry-ou/kiwi-browser-fast-quiet/',
#                'netguard':'apk/marcel-bokhorst/netguard-no-root-firewall/',
#                'afwall':'apk/portgenix/afwall-android-firewall/',
#                'k9':'apk/k-9-dog-walkers/k-9-mail/',
#                'docs':'apk/google-inc/docs/',
#                'sheets':'apk/google-inc/sheets/',
#                'maps':'apk/google-inc/sheets/',
#                 'moovit':'apk/moovit/moovit-bus-train-live-info/',
#                'facebook':'apk/facebook-2/facebook/',
#                 'IG':'apk/instagram/instagram-instagram/',
#                'poweramp':'apk/max-mp/poweramp/',
#                 'vlc':'apk/videolabs/vlc/',
#                'mega':'apk/mega-ltd/mega-official/',
#                'youtube':'apk/google-inc/youtube/',
#                'whatsapp':'apk/whatsapp-inc/whatsapp/',
#                'telegram': 'apk/telegram-fz-llc/telegram/',
#                'discord': 'apk/discord-inc/discord-chat-for-gamers/',
#                'steam': 'apk/valve-corporation/',
#                '': '',
#                '': '',
#                '': '',
#
#
#
#
# }

def mainScrape(listOfApps,beta):
    for App in listOfApps:
        #downloadWebSite,downloadLink = searchForApp(App)
        download = data[App]
        #downloadListTemp = download.split('/')
        #downloadparse = urlparse(download)
        #downloadWebSite =  downloadparse.hostname
        if download.startswith('https://www.apkmirror.com/'):
            downloadAPKMirror(download,beta)
        elif download.startswith('https://github.com/'):
            downloadGitHub(download)



def initSelenium():
    #################Init######################
    local_state = {
        "dns_over_https.mode": "secure",
        "dns_over_https.templates": "https://dns.adguard-dns.com/dns-query",
    }
    options = Options()
    # options.add_argument("--headless")
    os.getcwd()


    prefs = {"profile.default_content_settings.popups": 0,
             "download.default_directory": download_dir,  ### Set the path accordingly
             "download.prompt_for_download": False,  ## change the downpath accordingly
             "download.directory_upgrade": True}
    options.add_experimental_option("prefs", prefs)

    options.add_experimental_option('localState', local_state)
    driver = webdriver.Chrome(options=options)
    return driver

def downloadGitHub(downloadLink):
    driver = initSelenium()
    driver.get(downloadLink)
    driver.execute_script(f"window.scrollTo(0, 800);")
    time.sleep(1)
    downloadHREF = driver.find_element(By.PARTIAL_LINK_TEXT,"apk").get_attribute("href")
    fileNameVersion = downloadHREF.split('/')[-1]
    session = requests.Session()
    response = session.get(downloadHREF)
    saveFile(fileNameVersion,response,driver)

def downloadAPKMirror(downloadLink,beta):
    driver,download_dir = initSelenium()

    driver.get(downloadLink)
    seleniumUserAgent = driver.execute_script("return navigator.userAgent")
    page_height = driver.execute_script("return document.body.scrollHeight")
    driver.execute_script(f"window.scrollTo(0, {page_height / 5});")
    #time.sleep(1)

    listOfAvailVersions = driver.find_element(By.XPATH,"/html/body/div[2]/div/div[1]/div[5]").find_elements(By.TAG_NAME,'h5')

    if (beta):
        listOfAvailVersions[0].click()
    else:
        for div_element in listOfAvailVersions:
            text_content = div_element.text
            if "beta" not in text_content:
                div_element.click()
                break

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
    latest_download = max(os.listdir(download_dir), key=lambda x: os.path.getctime(os.path.join(download_dir, x)))
    saveFile(fileNameVersion + ".apk",response,driver)
    os.remove(os.path.join(download_dir, latest_download))
    # Close the WebDriver session when done


def saveFile(fileNameVersion,response,driver):
    #check if response is succeful and only then continue
    if fileNameVersion == "app-release.apk":
        fileNameVersion = driver.find_element(By.XPATH,"""//*[@id="repo-content-pjax-container"]/div/div/div/div[1]/div[1]/div[1]/div[1]/h1""").text + ".apk"
    local_file_path = "apks/" + fileNameVersion
    with open(local_file_path, 'wb') as file:
        file.write(response.content)
    driver.quit()

    print('succefully downloaded ' + fileNameVersion)

# def searchForApp(nameOfApp):
#     for website in data['websites']:
#         try:
#             if website['apps'][nameOfApp]:
#                 return website['url'],website['apps'][nameOfApp]
#         except:
#             continue




if __name__ == "__main__":
    listOfApps = ["droidify","vimusic","newpipexsponserblock","adaway","rethink"]
    beta = True
    mainScrape(listOfApps, beta)