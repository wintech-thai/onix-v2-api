#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

url = "https://register-dev.please-scan.com/napbiotec/customer-user-create/591d31a6-b581-4f0d-8ee1-3a1d74e6c5a3?data=eyJFbWFpbCI6InBqYW1lLmZiQGdtYWlsLmNvbSIsIk5hbWUiOiJTZXVicG9uZyBNb25zYXIiLCJDb2RlIjoiQ1VTVDo2Y2M2Y2ZjZS05MzliLTRmODctOWE1OS0wM2ZhZmJiMWZjZTMiLCJJZCI6IjM0MmM5NWRiLTZmNDgtNDU5Zi05Zjc0LWYwZWZmNmVlYTcxYyJ9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

api = "ConfirmCreateCustomerUser"

#puts(dataObj)
custId = dataObj['Id']

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  Password: "#{ENV['USER_PASSWORD']}",
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{custId}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
