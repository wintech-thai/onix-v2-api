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

url = "https://register-dev.please-scan.com/napbiotec/user-signup-confirm/26e069a7-850d-47a8-86e2-92f678292d08?data=eyJFbWFpbCI6ImF1dHR6bWFyZ2FyZXR6QGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoiYXV0dHptYXJnYXJldHoiLCJQYXNzd29yZCI6bnVsbCwiTmFtZSI6bnVsbCwiTGFzdG5hbWUiOm51bGwsIkludml0ZWRCeSI6InNldWJwb25nLm1vbiIsIk9yZ1VzZXJJZCI6ImQ1Y2IyY2NmLTE3ZTEtNGJlZi1hYjBjLTNjNzgzNGM0NDRkOSJ9"
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

api = "ConfirmNewUserInvitation"
if (regType != 'user-signup-confirm')
  api = "ConfirmExistingUserInvitation"
end

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "Abc12345$343#1",
  Name: "Supreecha",
  LastName: "Monsar",
  OrgUserId: dataObj['OrgUserId'],
}

#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
