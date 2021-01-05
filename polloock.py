# coding=utf-8

from typing import List
import random
import asyncio
import datetime

import discord
from discord.ext.commands import Bot

from Cryptodome import Random
from Cryptodome.Cipher import AES
import base64

import httpx
from bs4 import BeautifulSoup

bot = Bot(command_prefix="")

#region 폴록봇 변수
#region 대화 관련
POLLOCK_QUESTION = ["예?", "왜요?", "왜 불렀?"]
POLLOCK_BYE = ["안녕히 가세요!", "다음에 또 봐요!", "잘가요요오오오!", "ㅇㅇ ㅂㅂ", "넹 다음에 또 뵈요!", "넵 주인님 내일 뵈요!"]
POLLOCK_HI = ["넹 안녕하세요!", "오 안녕하세요!", "안녕하세요요오오오!", "ㅇㅇ ㅎㅇ!", "안뇽!", "하이루!", "오 왔구나", "왜또왔..."]
POLLOCK_JOKE = ["2마리의 용으로 담은 술은 거꾸로 하면? = ||이주용 쿠쿠루쿠쿠쿠쿠쿠||", "1더하기1은?(이것도 모르면 인간머리 수듄;;) =||3||.","아름다움은 어떤원소로 가득차 있을까?=||BE(베릴륨),au(금),티타늄(ti)+full(가득)=beatiful||"]
POLLOCK_IDK = ["뭐라고요?..", "그게 뭐죠", ":thinking:", "?"]
POLLOCK_INFORM_TIME = ["이야. 제발 시간을 낭비하지 않길 바래.", "입니다!", "인데 시간이 금인거 몰라?"]
POLLOCK_WRITTEN = ["'님이 알려주셨어요!`", "'휴먼이 받아적으라고 협박했어요 TT`", "'동무가 알려주었다우`", "'고맙소`"]
#endregion
#region 기타
POLLOCK_TOKEN = "MY_TOKEN"

TEAM_ID = [0, 0, 0]

MY_AES_KEY = "BANANAMNANAJIJANJAJSANJSDNJDSJNJ"
#endregion
#endregion

@bot.event
async def on_ready():
    await bot.change_presence(status=discord.Status.online, activity=discord.Game("세상에서 가장 인성 터진 디코 봇!"))
    print("부릉! 부릉!!!")

@bot.event
async def on_command_error(context: discord.ext.commands.context.Context, exception):  # 오류 처리
    if get_class_path(exception) == "discord.ext.commands.errors.CommandNotFound":
        return
    else:
        embed = get_error_message_for_embed(context, exception)
        await context.send(embed=embed)

@bot.command(name="폴록아")
async def hey_pollock(ctx: discord.ext.commands.context.Context, *args):
    if not bot.is_ready():
        await bot.wait_until_ready()

    arg = list_to_str(list(args))
    commands = list(args)

    if len(args) == 0:
        await ctx.send(random.choice(POLLOCK_QUESTION))
    elif arg.startswith("안녕") or arg.startswith("하이") or arg.startswith("ㅎㅇ") or arg.startswith("ㅎ2") or arg.startswith("방가"):
        await ctx.send(random.choice(POLLOCK_HI))
    elif arg.startswith("잘가") or arg.startswith("ㅂㅂ") or arg.startswith("ㅂㅇ") or arg.startswith("ㅂ2") or arg.startswith("ㅃ") or arg.startswith("ㅂ"):
        await ctx.send(random.choice(POLLOCK_BYE))

    elif arg.startswith("테스트"):
        text = arg.lstrip("테스트 ")
        text_list = text.split(" ")

        await ctx.send("Hello, World!")
        await ctx.message.add_reaction("🤔")

    elif arg.startswith("시간"):
        time = (datetime.datetime.utcnow() + datetime.timedelta(hours=9)).strftime("%H시 %M분".encode('unicode-escape').decode()).encode().decode('unicode-escape')
        await ctx.send("지금은 " + time + " " + random.choice(POLLOCK_INFORM_TIME))

    elif arg.startswith("한강"):  # 현재 한강 온도 확인 사이트를 바꿈.
        headers = {
            "user-agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36"
        }

        async with httpx.AsyncClient() as httpClient:
            hangang = await httpClient.get("https://hangang.life", headers=headers)
            soup = BeautifulSoup(hangang.text, 'html.parser')
            bs = soup.find_all("h1", {"class": "white"})[1]

            temperature = bs.text
            date = datetime.datetime.strptime(bs["onclick"].lstrip("UIkit.notification({message: '").rstrip("'})"), "%Y-%m-%d %H:%M:%S").strftime("%Y년 %m월 %d일 %H시 %M분".encode('unicode-escape').decode()).encode().decode('unicode-escape')
            wise_sayings = ["본성이 우리에게 준 가장 훌륭한 선물은 삶으로부터 도망치게 내버려둔다는 점이다.", "자신의 종말을 기다리는 사람은 엄격한 영혼을 가졌다기보다는 오히려 본성이 감퇴된 것이 아닐까?", "겨울이 오면 봄이 멀지 않으리.", "내 비장의 무기는 아직 손안에 있다, 그것은 희망이다.", "대부분의 사람들은 고요한 절망 속에서 인생을 살아간다.", "막다른 곳에 빠지게 될 때에는 온몸을 바쳐 부딪쳐라.", "모든 것을 손에 넣으면 희망이 사라진다, 언제나 어느 정도의 욕심과 희망을 비축해두라.", "모든 희망은 인간에게 허락되어 있다, 세상에서 사라져 버리겠다는 희망까지도.", "몹시 좌절될 것 같이 여겨지는 사건이 전화위복으로 그 사람의 인생에 최대의 분기점이 되는 경우가 있다. 전화위복의 기회는 항상 있다.", "비록 내일 세계의 종말이 온다 할지라도, 나는 오늘 한 그루의 사과나무를 심겠다.", "비참한 인간들에겐 희망이 약이다.", "삶에 대한 절망 없이는 삶에 대한 희망도 없다.", "역경은 희망에 의해서 극복된다", "우리들은 감탄과 희망과 사랑으로 산다.", "우리들은 과거에의 집착보다 미래의 희망으로 살고 있다.", "안심하는걸 불안해 하는 나를 홀려.", "꿈을 현실로.", "우리들은 소망해선 안될 것을 가장 소망한다.", "위대한 희망은 위대한 인물을 만든다, 산은 오르는 사람에게만 정복된다.", "인간이 절망하는 곳에는 어떠한 신도 살 수 없다.", "인내하라, 경험하라, 조심하라. 그리고 희망을 가져라.", "잠이 꿈을 주듯, 바다는 사람에게 희망을 준다.", "절망으로부터의 유일한 피난처는 세상에 자아를 포기하는 것이다.", "인생은 유희가 아니다, 그러므로 우리들에겐 자기만의 의사로 이것을 포기할 권리는 없다.", "절망은 죽음에 이르는 병이다, 쉽게 절망하여 포기하면 마음까지 해친다.", "희망은 그것을 추구하는 사람을 결코 내버려두지는 않는다.", "희망은 사람을 성공으로 이끄는 신앙이다. 희망이 없으면, 아무 것도 성취할 수가 없으며 희망없이는 인간생활이 영위될 수 없다.", "희망은 일생의 어떠한 때도 우리들을 버리지 않는다.", "자살은 심각한 고민이 낳는 가장 두려운 증상이다.", "죽고 싶은 생각이 들면 일을 하라", "죽으려고 하기보다는 살려고 하는 편이 더 용기를 필요로 하는 시련이다."]
            wise_saying = random.choice(wise_sayings)

            await ctx.send(f"아, 한강 온도 말하는거야? 방금 측정해보니\n{date} 기준, 한강 온도는 {temperature}야.\n\n`{wise_saying} (by WinSub)`")

    elif arg == "자폭":
        await ctx.send("다함께 같이 폭사하자.")
        await asyncio.sleep(2)
        await ctx.send("💥")
        raise NameError("다함께 같이 폭사하자. 💥")

    elif not is_team(ctx.author.id) or not arg.startswith("개발자모드"):
        await ctx.send(random.choice(POLLOCK_IDK))



    if is_team(ctx.author.id):  # 개발자 전용 명령어
        if arg.startswith("개발자모드 오류코드 복호화 "):
            cipher_text = commands[3]
            plain_text = AES256.decrypt(cipher_text, MY_AES_KEY)

            await ctx.send("오류 코드 메시지:\n```" + plain_text + "```")

        else:
            await ctx.send(random.choice(POLLOCK_IDK))

#region 폴록봇 함수
#region 게임 관련 함수

#endregion
#region 디스코드 관련 함수
def is_team(id: int) -> bool:
    '''
    해당 유저가 팀에 속하는지의 여부입니다.
    :param id: 유저 아이디
    :return: 팀에 속하는지의 여부
    '''

    return id in TEAM_ID

def get_command_for_CommandNotFoundError(error_message: str) -> str:
    '''
    CommandNotFoundError의 오류 메시지에서 해당 명령어를 반환합니다.

    :param error_message: 오류 메시지
    :return: 명령어
    '''

    return error_message.lstrip('Command "').rstrip('" is not found')

def get_error_message_for_embed(ctx: discord.ext.commands.context.Context, error) -> discord.Embed:
    exception_message = str(error) + "\n" + "messageContent: " + ctx.message.content
    encrypt_text = AES256.encrypt(exception_message, MY_AES_KEY)

    embed = discord.Embed(title=":(", description="폴록봇이 자폭되었습니다.\n\n`" + encrypt_text + "`\n위에 있는 코드를 복사하여 봇 개발자에게 보내주세요.\n")
    embed.set_footer(text="MineEric64", icon_url=bot.get_user(0).avatar_url)

    return embed
#endregion
#region 기타 함수
def get_class_path(object) -> str:
    '''
    클래스의 경로를 반환합니다.

    :param object: 변수
    :return: 클래스 경로
    '''

    return str(type(object)).lstrip("<class '").rstrip("'>")

def list_to_str(l: list, point = " ") -> str:
    '''
    리스트를 문자열로 바꿔줍니다.

    :param l: 리스트
    :param point: 문자열에 추가할 변수
    :return: 리스트 내에 들어있는 문자열
    '''

    x = ""

    for i in range(0, len(l)):
        x += str(l[i])

        if i != len(l) - 1:
            x += point

    return x

def remove_objects_from_list(l: list, remove_list: list):
    '''
    리스트에 있는 오브젝트들을 제거 합니다.

    :param l: 리스트
    :return: 리스트
    '''

    for x in remove_list:
        l.remove(x)

class AES256:
    @staticmethod
    def encrypt(x: str, key: str) -> str:
        raw = AES256.__pad(x)
        iv = Random.new().read(AES.block_size)
        cipher = AES.new(key.encode("utf-8"), AES.MODE_CBC, iv)
        return base64.b64encode(iv + cipher.encrypt(raw.encode('utf-8'))).decode('utf-8')

    @staticmethod
    def decrypt(x: str, key: str) -> str:
        enc = base64.b64decode(x)
        iv = enc[:16]
        cipher = AES.new(key.encode("utf-8"), AES.MODE_CBC, iv)
        return AES256.__unpad(cipher.decrypt(enc[16:])).decode('utf-8')

    @staticmethod
    def __pad(s: str) -> str:
        BS = 16
        return s + (BS - len(s.encode('utf-8')) % BS) * chr(BS - len(s.encode('utf-8')) % BS)

    def __unpad(s: bytes) -> bytes:
        return s[:-ord(s[len(s) - 1:])]
#endregion
#endregion

bot.run(POLLOCK_TOKEN)