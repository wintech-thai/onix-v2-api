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

# ลิงค์ที่มาจาก API จะขึ้นต้นด้วย user-signup-confirm ก่อน orgId แต่เวลาเรียก API signup อีกทีจะเป็น orgId ก่อน user-signup-confirm
url = "https://web-dev.please-payment.com/ppm-alfa888/user-signup-confirm/ce6a7eff-04ab-4dfd-8b6c-771acea4e971?data=eyJFbWFpbCI6InBqYW1lLmZiM0BnbWFpbC5jb20iLCJVc2VyTmFtZSI6InNldWJwb25nMy5tb24iLCJQYXNzd29yZCI6bnVsbCwiTmFtZSI6bnVsbCwiTGFzdG5hbWUiOm51bGwsIkludml0ZWRCeSI6InNldWJwb25nLm1vbiIsIk9yZ1VzZXJJZCI6ImRiODYxNzJlLWU0YzMtNGUyZi04MmI0LWYyMTNhN2QzMjkzMSIsIk9yZ1R5cGUiOm51bGx9"
#"https://register-dev.please-scan.com/pjame16/user-invite-confirm/73512d27-f92c-41dc-b780-c91a2d08fce8?data=eyJFbWFpbCI6InRlc3RpbmdAeHlzYS5jb20iLCJVc2VyTmFtZSI6ImFkaXNvcm4ucCIsIlBhc3N3b3JkIjpudWxsLCJOYW1lIjpudWxsLCJMYXN0bmFtZSI6bnVsbCwiSW52aXRlZEJ5Ijoic2V1YnBvbmcubW9uIiwiT3JnVXNlcklkIjoiZTNlNDdiN2QtMzAyMy00Zjk4LThmYzktOTcwZmEzZjM2NTliIn0%3d"
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
